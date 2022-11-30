module Demo911App.Account

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open IntelliFactory.WebSharper.Google
open IntelliFactory.WebSharper.Google.Visualization
open IntelliFactory.WebSharper.Google.Visualization.Base
open Helpers
          
type LoginInfo = { UserName: string; Password: string }
type LoginControl() = 
    inherit Control()    
    
    [<JavaScript>]
    let LoginForm() =
        let uName =
            Controls.Input ""
            |> Validator.IsNotEmpty "Enter Username"
            |> Enhance.WithTextLabel "Username"
        let pw =
            Controls.Password ""
            |> Validator.IsNotEmpty "Enter Password"
            |> Enhance.WithTextLabel "Password"
        Formlet.Yield (fun n pw -> { UserName=n; Password=pw })
        <*> uName <*> pw
        |> Enhance.WithCustomSubmitButton
            ({Enhance.FormButtonConfiguration.Default with Label=Some("Login"); Style=Some("background-color:#04C;color:#fff; margin-left:78px; padding:6px; width:262px")})
        |> Enhance.WithFormContainer 
                    

    [<JavaScript>]
    let doLogin user pass (label: Element) = async {
        try
            let! res = Api.Login user pass
            if res.Success then
                let i = res.Data.IndexOf(':')
                let aid = res.Data.Substring(0, i)
                Api.Vars.SetAccountId <| int aid
                Api.Vars.SetApiKey <| res.Data.Substring(i+1)
                Api.Vars.SetAccountName ""
                Api.Vars.SetIsAdmin "false"
                let! res = Api.GetAccount (int Api.Vars.AccountId)
                label.Text <- ""
            else 
                label.Text <- "Invalid username or password."            
        with ex -> label.Text <- ex.Message
    }
        
    [<JavaScript>]
    override this.Body = 
        let errorBox = Div[Attr.Class "alert alert-error span hide"; Attr.Style "margin-left:92px; margin-bottom:12px;"]
        let errorLabel = Label[Attr.Class ""]
        
        let l = LoginForm().Run(fun li -> Async.Start (async {
                                        JE(errorBox).Hide() |> ignore
                                        do! doLogin li.UserName li.Password errorLabel
                                        if errorLabel.Text = "" then 
                                            let isLoginOverlay = JS("#LoginOverlay:visible").Length > 0;
                                            if isLoginOverlay then
                                                JS("#LoginOverlay").FadeOut().Ignore
                                                refresh()
                                            else 
                                                redirect Api.Action.Dashboard
                                        else JE(errorBox).Show() |> ignore
                                    }
                                ))
        JS("document").Ready(fun _ -> 
            JS(".formlet").Keyup(fun _ k -> if k.Which = 13 then JS(".submitButton").Click() |> ignore) |> ignore 
            if Html5.Window.Self.Location.Pathname.Contains("/Login") then JS(".nav").Hide() |> ignore
            JS(".inputText").First().Focus() |> ignore
        ) |> ignore
        

        upcast (Div [Attr.Class "well padT2"]
                    -< [H4[Text "Login"; Attr.Class "padL blue"]] 
                    -< [l]
                    -< [errorBox -< [errorLabel]; Cl()])
        
type LogoutControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let logoutBox = Div[Text "Logging out..."; Attr.Class "gray bold"]
        Async.Start (async{
                            let! res = Api.Logout
                            if res.Success then
                                if Api.Vars.IsAdmin then redirect Api.Action.AdminDashboard else redirect Api.Action.Login
                            else //try agian
                                let! res2 = Api.Logout
                                if Api.Vars.IsAdmin then redirect Api.Action.AdminDashboard else redirect Api.Action.Login
                        }
                    )
        upcast (Div [logoutBox])
 
type DashboardControl() = 
    inherit Control()    
        
    [<JavaScript>]
    let AddressChartData () =
        let data = new Base.DataTable()
        data.addColumn(ColumnType.StringType, "Type") |> ignore
        data.addColumn(ColumnType.NumberType, "Addresses") |> ignore
        data.addRows(3) |> ignore
        data.setValue(0, 0, "Valid")
        data.setValue(0, 1, 321)
        data.setValue(1, 0, "Potentially Valid")
        data.setValue(1, 1, 25)
        data.setValue(2, 0, "Invalid")
        data.setValue(2, 1, 7)
            
        data

    [<JavaScript>]
    let AddressChart () =
        Div []
         |>! OnAfterRender (fun container ->
            let visualization = new Visualizations.ColumnChart(container.Dom)
            let axis =  Visualization.Base.Axis()
            axis.minValue <- 0.
            axis.baseline <- 0
            axis.logScale <- false
            let options = { Visualizations.ColumnChartOptions.Default with
                             width = 400.
                             height = 240.
                             legend = Visualizations.LegendPosition.Bottom
                             title = "SAMPLE"
                             vAxis = axis
                             hAxis = axis                                                      
                           }
                           
            visualization.draw(AddressChartData(), options))
    
    [<JavaScript>]
    let CallsCharData () =
        let data = new Base.DataTable()
        data.addColumn(ColumnType.StringType, "Day") |> ignore
        data.addColumn(ColumnType.NumberType, "Calls") |> ignore
        data.addRows(7) |> ignore
        data.setValue(0, 0, "Sun")
        data.setValue(0, 1, 1)
        data.setValue(1, 0, "Mon")
        data.setValue(1, 1, 4)
        data.setValue(2, 0, "Tue")
        data.setValue(2, 1, 5)
        data.setValue(3, 0, "Wed")
        data.setValue(3, 1, 3)
        data.setValue(4, 0, "Thu")
        data.setValue(4, 1, 2)
        data.setValue(5, 0, "Fri")
        data.setValue(5, 1, 3)
        data.setValue(6, 0, "Sat")
        data.setValue(6, 1, 7)
    
        data
    
    [<JavaScript>]
    let CallsChart () =
        Div [Attr.Class "rounded"]
          |>! OnAfterRender (fun container ->
            let visualization = new Visualizations.AreaChart(container.Dom)
            let axis =  Visualization.Base.Axis()
            axis.minValue <- 0.
            axis.baseline <- 0
            let options = {
                Visualizations.AreaChartOptions.Default with
                    width = 400.
                    height = 240.
                    legend = Visualizations.LegendPosition.Bottom
                    title = "SAMPLE"
                    vAxis = axis
                    hAxis = axis
            }    
            visualization.draw(CallsCharData(), options))


    [<JavaScript>]
    let StateCallMapData () =
        let data = new Base.DataTable()
        data.addRows(48) |> ignore
        data.addColumn(ColumnType.StringType, "State") |> ignore
        data.addColumn(ColumnType.NumberType, "Addresses") |> ignore

        data.setValue(0, 0, "New York")
        data.setValue(0, 1, 40)
        data.setValue(1, 0, "Colorado")
        data.setValue(1, 1, 42)
        data.setValue(2, 0, "Florida")
        data.setValue(2, 1, 34)
        data.setValue(3, 0, "Texas")
        data.setValue(3, 1, 28)
        data.setValue(4, 0, "California")
        data.setValue(4, 1, 23)
        data.setValue(5, 0, "Washington")
        data.setValue(5, 1, 23)
        data.setValue(6, 0, "Georgia")
        data.setValue(6, 1, 26)
        data.setValue(7, 0, "Oregon")
        data.setValue(7, 1, 15)
        data.setValue(8, 0, "District of Columbia")
        data.setValue(8, 1, 20)
        data.setValue(9, 0, "Nebraska")
        data.setValue(9, 1, 12)
        data.setValue(10, 0, "Michigan")
        data.setValue(10, 1, 30)
        data.setValue(11, 0, "Illinois")
        data.setValue(11, 1, 9)
        data.setValue(12, 0, "Minnesota")
        data.setValue(12, 1, 17)
        data.setValue(13, 0, "North Dakota")
        data.setValue(13, 1, 30)
        data.setValue(14, 0, "Maine")
        data.setValue(14, 1, 23)
        data.setValue(15, 0, "Delaware")
        data.setValue(15, 1, 22)
        data.setValue(15, 0, "Connecticut")
        data.setValue(15, 1, 3)
        data.setValue(16, 0, "Maryland")
        data.setValue(16, 1, 12)
        data.setValue(17, 0, "Virginia")
        data.setValue(17, 1, 43)
        data.setValue(18, 0, "North Carolina")
        data.setValue(18, 1, 13)
        data.setValue(19, 0, "South Carolina")
        data.setValue(19, 1, 23)
        data.setValue(20, 0, "Louisiana")
        data.setValue(20, 1, 33)
        data.setValue(21, 0, "Arizona")
        data.setValue(21, 1, 36)
        data.setValue(22, 0, "New Mexico")
        data.setValue(22, 1, 13)
        data.setValue(23, 0, "Montana")
        data.setValue(23, 1, 21)
        data.setValue(24, 0, "Mississippi")
        data.setValue(24, 1, 26)
        data.setValue(25, 0, "Kansas")
        data.setValue(25, 1, 21)
        data.setValue(26, 0, "Oklahoma")
        data.setValue(26, 1, 12)
        data.setValue(27, 0, "Idaho")
        data.setValue(27, 1, 43)
        data.setValue(28, 0, "Missouri")
        data.setValue(28, 1, 13)
        data.setValue(29, 0, "Vermont")
        data.setValue(29, 1, 23)
        data.setValue(30, 0, "Indiana")
        data.setValue(30, 1, 33)
        data.setValue(31, 0, "Wyoming")
        data.setValue(31, 1, 36)
        data.setValue(32, 0, "Iowa")
        data.setValue(32, 1, 13)
        data.setValue(33, 0, "Ohio")
        data.setValue(33, 1, 21)
        data.setValue(34, 0, "Arkansas")
        data.setValue(34, 1, 26)
        data.setValue(35, 0, "West Virginia")
        data.setValue(35, 1, 21)
        data.setValue(36, 0, "Kentucky")
        data.setValue(36, 1, 43)
        data.setValue(37, 0, "South Dakota")
        data.setValue(37, 1, 13)
        data.setValue(38, 0, "Pennsylvania")
        data.setValue(38, 1, 23)
        data.setValue(39, 0, "Rhode Island")
        data.setValue(39, 1, 33)
        data.setValue(40, 0, "Utah")
        data.setValue(40, 1, 36)
        data.setValue(41, 0, "Wisconsin")
        data.setValue(41, 1, 13)
        data.setValue(42, 0, "New Hampshire")
        data.setValue(42, 1, 21)
        data.setValue(43, 0, "Tennessee")
        data.setValue(43, 1, 26)
        data.setValue(44, 0, "New Jersey")
        data.setValue(44, 1, 21)
        data.setValue(45, 0, "Alabama")
        data.setValue(45, 1, 28)
        data.setValue(46, 0, "Nevada")
        data.setValue(46, 1, 38)

        data
        
    [<JavaScript>]
    let StateCallMap () =
        Div [Attr.Class "marL rounded"]
            |>! OnAfterRender (fun container ->
            let visualization = new Visualizations.GeoMap(container.Dom)
        
            let options = {
        
                Visualizations.GeoMapOptions.Default with
                    width = "400px"
                    height = "200px"
                    region = Visualizations.Region.FromString "US"
                    colors = [| 0x98E0F5; 0x2260BA |]
                    dataMode = Visualizations.DataMode.Regions

        
            }
            visualization.draw(StateCallMapData(), options))


    [<JavaScript>]
    override this.Body = 
        let parent = Div[Attr.Class "well up6";] 
        //account fields
        let ContactName = TextboxTip "Name" "" "input-medium" ""
        let ContactCompany = TextboxTip "Company" "" "input-large" ""
        let Email =  TextboxTip "Email" "" "input-medium" ""
        let ContactAddress = TextboxTip "Address" "" "" ""
        let ContactCity = TextboxTip "City" "" "" ""
        let ContactState = TextboxTip "State" "" "input-mini" ""
        let ContactPostal = TextboxTip "Postal" "" "input-small" ""
        let ContactCountry = Dropdown "Country" "" "" "input-medium" ["US","United States";"CA","Canada";]
        let saveBtn = Button[Text "Save Changes"; Attr.Class "btn btn-primary";]
                
        //readonly if not admin
        if not Api.Vars.IsAdmin then
            JD(ContactName.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore
            JD(ContactCompany.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore
            JD(Email.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore
            JD(ContactAddress.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore
            JD(ContactCity.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore
            JD(ContactState.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore
            JD(ContactPostal.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore
            JD(ContactCountry.Dom.LastChild).Attr("readonly","readonly").Css("background-color","#fff").Ignore

            JE(saveBtn).Hide().Ignore

            //input filtering
            JD(ContactState.Dom.LastChild).Attr("maxlength", "2").Ignore
            JD(ContactPostal.Dom.LastChild).Attr("maxlength", "10").Ignore

        //change password fields
        let passInputs = Span[Attr.Class "size-2 hide"]
        let oldPass = TippedInput "Old Password" "Your current password" "password" "input-small" ""
        let newPass = TippedInput "New Password" "New password, must be at least 8 characters long" "password" "input-small" ""
        let confPass = TippedInput "Confirm New" "Must match new password" "password" "input-small" ""
        let changePassBtn = Button[Text "Change Password"; Attr.Style "margin-top:20px;"; Attr.Class "btn";]
        let interopLink = A[Text "View Interop Reference"; Attr.Class "bold"; Attr.HRef "https://www.___.com\Home\Interop"; Attr.Target "_blank";]
        let apiLink = A[Text "View API Docs"; Attr.Class "bold marL2"; Attr.HRef "https://api.___.com/v1/"; Attr.Target "_blank";]
        let bsLink = A[Text "View Branded Site Docs"; Attr.Class "bold marL2"; Attr.HRef "https://api.___.com/v1/brandedsitedocs.html"; Attr.Target "_blank";]
        
        //get data
        let acc = ref Api.Account.Empty
        Async.Start (async{
                            let! res = Api.GetAccount (int Api.Vars.AccountId)
                            if res.Success then
                                acc := res.Data
                                SetInputVal Email res.Data.Email |> ignore
                                SetInputVal ContactName res.Data.ContactName |> ignore
                                SetInputVal ContactCompany res.Data.ContactCompany |> ignore
                                SetInputVal ContactAddress res.Data.ContactAddress |> ignore
                                SetInputVal ContactCity res.Data.ContactCity |> ignore
                                SetInputVal ContactState res.Data.ContactState |> ignore
                                SetInputVal ContactPostal res.Data.ContactPostal |> ignore
                                SetInputVal ContactCountry res.Data.ContactCountry |> ignore
                            else ErrorAlert res.ErrorInfo
                        }
                    )

        //js        
        JD(ContactCountry.Dom.LastChild).Click(fun x _ -> J(x).Val("US") |> ignore) |> ignore
        JD(Email.Dom.LastChild).Attr("readonly", "readonly") |> ignore
        let jsaveBtn = JE(saveBtn)
        jsaveBtn.Click(fun _ _ ->
                    Async.Start(
                        async {
                            ClearMessageAlerts() 
                            
                            //validate
                            let valid = Required parent
                                            [
                                                ContactName
                                                ContactCompany
                                                ContactAddress
                                                ContactCity
                                                ContactState
                                                ContactPostal
                                                ContactCountry
                                            ]

                            if !valid then
                                jsaveBtn.Attr("disabled","disabled") |> ignore
                                                        
                                let editedAcc = {!acc with
                                                    ContactName = GetInputVal ContactName;
                                                    ContactCompany = GetInputVal ContactCompany;
                                                    ContactAddress = GetInputVal ContactAddress;
                                                    ContactCity = GetInputVal ContactCity;
                                                    ContactState = GetInputVal ContactState;
                                                    ContactPostal = GetInputVal ContactPostal;
                                                    ContactCountry = GetInputVal ContactCountry;
                                                }
                                let! res = Api.EditAccount editedAcc
                                match res with
                                | z when z.Success ->
                                    Success "Changes saved" parent
                                | z ->
                                    Error ("Unable to save changes: "+ z.ErrorInfo) parent
                                jsaveBtn.Delay(2000).RemoveAttr("disabled") |> ignore
                      } 
                    )
                  ) |> ignore
        
        let jchangePassBtn = JE(changePassBtn)
        let passOpen = ref false
        jchangePassBtn.Click(fun _ _ ->
                    Async.Start(
                        async {
                            ClearMessageAlerts() 
                            if !passOpen then
                            
                                //validate
                                let valid = Required parent
                                                [
                                                    oldPass
                                                    newPass
                                                ]

                                if !valid then
                                    let oldPassVal = GetInputVal oldPass
                                    let newPassVal = GetInputVal newPass
                                    let confPassVal = GetInputVal confPass
                                    if not(newPassVal = confPassVal) then Error "Passwords do not match" parent
                                    else
                                        if (newPassVal.Length.ToString() = "undefined" || newPassVal.Length < 8) then Error "Password must be at least 8 characters long" parent
                                        else
                                            let! res = (Api.ChangePassword oldPassVal newPassVal)
                                            match res with
                                            | z when z.Success ->
                                                Success "New password saved" parent
                                                SetInputVal oldPass "" |> ignore
                                                SetInputVal newPass "" |> ignore
                                                SetInputVal confPass "" |> ignore
                                                jchangePassBtn.RemoveClass("btn-primary") |> ignore
                                                JE(passInputs).Hide() |> ignore
                                            | z ->
                                                Error ("Unable to save password: "+ z.ErrorInfo) parent
                                            passOpen := false
                            else
                                passOpen := true
                                jchangePassBtn.AddClass("btn-primary") |> ignore
                                JE(passInputs).Show() |> ignore
                        }
                    )
                ) |> ignore
                    
        //compose
        authScreen <|
            parent 
                -< [
                    Div[Attr.Class (if Api.Vars.AccountId = "1071" then "hide" else "hide")] -< [
                        Div[Attr.Class "right"; Attr.Style "text-align:center; margin-top:-20px; margin-right:-20px;"]
                            -< [                                                  
                                Div[Attr.Class "size2 alert alert-success"; Attr.Style "width:500px"]
                                    -< [
                                            B[ Attr.Class ""; Text "Current 9-1-1 SIP status:";]
                                            B[ Attr.Class "marL2"; Text "LA:"]
                                            Span[Attr.Class "marL badge badge-success"; Text "Active"]
                                            B[ Attr.Class "marL2"; Text "Denver:"]
                                            Span[Attr.Class "marL badge badge-success"; Text "Active"]
                                        ]
                                ]
                        Div[Attr.Class "span10"]
                            -< [                                                  
                                H3[Attr.Class "blue left"; Text "Statistics"; Attr.Style "margin-top:-40px;";]      
                                ]
                    
                        Div[Attr.Class "alert alert-info span10"]
                            -< [                               
                                Br[]       
                                Div[Attr.Class "left padL"]
                                    -< [
                                            B[ Attr.Class "blue"; Text "Total Addresses in System:"]
                                            AddressChart()
                                       ]
                                Div[Attr.Class "right";]
                                    -< [
                                            B[ Attr.Class "blue"; Text "9-1-1 Calls:"]
                                            CallsChart()
                                       ]
                                Cl()
                                Br[]
                                B[ Attr.Class "blue size1 marL"; Text "Addresses by State:"]
                                Div[ Attr.Class "marL"; Attr.Style "background-color:#eaf7fe; width:100%; text-align:center;"]
                                    -<[ Span[Attr.Class "left pad2"; Text "SAMPLE"]]
                                    -< [StateCallMap()]
                                Cl()
                            ]
                        Cll()
                        ]
                    
                    Div[Attr.Class "span10"]
                        -< [
                                H3[Attr.Class "blue"; Text "Company Profile"; Attr.Style "margin-top:-6px;";]
                                ContactName;
                                ContactCompany;
                                Email;
                                Span[ B[Text "Account ID "]; Br[]; Span[Text Api.Vars.AccountId; Attr.Class "size1"; Attr.Style "line-height:120%;";] ]
                                Cl();
                                ContactAddress;
                                ContactCity;
                                ContactState;
                                ContactPostal;
                                ContactCountry;
                                Cl();
                                Br[]
                                saveBtn;
                                Br[]
                                Span[] -< [
                                    passInputs -< [oldPass; newPass; confPass;]
                                    changePassBtn
                                ]
                                Br[];
                                Br[];
                                interopLink;
                                apiLink;
                                bsLink;
                                Cl();
                        ]
                    Cl();
                ]