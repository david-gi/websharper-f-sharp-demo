module Demo911App.Admin 

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
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
            let! res = Api.Admin.Login user pass
            if res.Success then
                let splt = res.Data.Split(':')
                Api.Vars.SetAccountId splt.[0]
                Api.Vars.SetApiKey splt.[1]
                Api.Vars.SetIsAdmin "true"
                Api.Vars.SetAccountName ""
                label.Text <- ""
                ()
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
                                        if errorLabel.Text = "" then redirect Api.Action.AdminDashboard
                                        else JE(errorBox).Show() |> ignore
                                    }
                                ))
        JS("document").Ready(fun _ -> 
            JS(".formlet").Keyup(fun _ k -> if k.Which = 13 then JS(".submitButton").Click() |> ignore) |> ignore 
            JS(".nav").Hide() |> ignore
            JS(".inputText").First().Focus() |> ignore
        ) |> ignore
        

        upcast (Div [Attr.Class "well padT2"]
                    -< [H4[Text "Admin Login"; Attr.Class "padL blue"]] 
                    -< [l]
                    -< [errorBox -< [errorLabel]; Cl()])
                
type LogoutControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let logoutBox = Div[Text "Logging out..."; Attr.Class "gray bold"]
        Async.Start (async{
                            let! res = Api.Admin.Logout
                            if not(res.Success) then 
                                let! res2 = Api.Admin.Logout
                                redirect Api.Action.AdminLogin
                            redirect Api.Action.AdminLogin
                        }
                    )
        upcast (Div [logoutBox])

type AccountsControl() = 
    inherit Control()    
                        
    [<JavaScript>]
    let ImpersonateControl() = 
        //Impersonate fields
        let parent = Div[] 
        let AccountId = Dropdown "" "Customer Account to log in as" "" "input-xlarge" ["","- Select -"]
        let btn = Button[Text "Go"; Attr.Class "btn btn-success"; Attr.Style "margin-top:20px;"]
        //get data
        let accounts = ref null        
        Async.Start(
            async {
                let! res = Api.Admin.ListAccounts()
                if res.Success then accounts := res.Data
                else ErrorAlert "Unable to load Accounts"
                let accountsDrop = JD(AccountId.Dom.LastChild)
                !accounts  |> Array.iter (fun (x: Api.Account) -> 
                        accountsDrop.Append("<option" + (if x.AccountId.ToString() = Api.Vars.AccountId then "selected='selected'" else "") +
                                            " value='"+(x.AccountId.ToString())+"'>"+x.ContactCompany + " (" + x.Email + ")</option>") |> ignore)
            }
        )
        //js
        JE(btn).Click(fun _ _ ->
                    Api.Vars.SetAccountName (JD(AccountId.Dom.LastChild).Find("option:selected").Text())
                    Api.Vars.SetAccountId (JD(AccountId.Dom.LastChild).Find("option:selected").Val())
                    redirect Api.Action.Dashboard
                ) |> ignore
                
        parent -< [B[Text "Login as customer account: ";Attr.Class "green left padR"; Attr.Style "font-size:1.2em;margin-top:25px;"]; AccountId; btn;]

    [<JavaScript>]
    override this.Body = 
        let parent = Div[Attr.Class "well up6";] 
        //account fields
        let ContactName = TextboxTip "Name" "Required" "input-medium" ""
        let ContactCompany = TextboxTip "Company" "Required" "input-large" ""
        let Email =  TextboxTip "Email" "Required" "input-medium" ""
        let ContactAddress = TextboxTip "Address" "Required" "" ""
        let ContactCity = TextboxTip "City" "Required" "" ""
        let ContactState = TextboxTip "State" "Required" "input-mini" ""
        let ContactPostal = TextboxTip "Postal" "Required" "input-small" ""
        let ContactCountry = Dropdown "Country" "Required" "" "input-medium" ["US","United States"; "CA","Canada";]
        let saveBtn = Button[Text "Save Changes"; Attr.Class "btn btn-primary"]
        let msgHolder = Div[]
        
        //TODO get data
        let tableHolder = Div[]
        let accounts = ref null        
        Async.Start(
            async {
                let! res = Api.Admin.ListAccounts()
                if res.Success then accounts := res.Data
                else ErrorAlert "Unable to load accounts"

                //table
                let rows =
                        !accounts |> Array.map(fun x -> 
                                let impBtn = Button[Text "Login as Customer"; Attr.Class "btn btn-success"]
                                JE(impBtn).Click(fun _ _ ->
                                                     Api.Vars.SetAccountId <| x.AccountId.ToString()
                                                     Api.Vars.SetAccountName (x.ContactCompany + "("+x.Email+")")
                                                     redirect Api.Action.Dashboard
                                                 ) |> ignore
                                                 
                                TR[
                                    TD[Text <| x.AccountId.ToString()]
                                    TD[Text <| screenUndefined x.ContactCompany]
                                    TD[Text <| screenUndefined  x.Email]
                                    TD[Text <| screenUndefined x.ContactName]
                                    TD[Text <| screenUndefined  (formatDate x.CreatedOn)]
                                    TD[impBtn]
                                ]
                            )
                        |> Array.toSeq
                let table = Table[Attr.Class "table table-striped"] 
                            -< [TR[TH[Text "ID"];TH[Text "Company"];TH[Text "Email"];TH[Text "Name"];TH[Text "Created"];TH[];]]
                            -< rows
                JE(tableHolder).Append(table.Dom) |> ignore
            }
        )
        
        //js
        let jsaveBtn = JE(saveBtn)
        jsaveBtn.Click(fun _ _ ->
                    Async.Start(
                        async {
                            ClearMessageAlerts() 
                            
                            //validate
                            let valid = Required msgHolder
                                            [
                                                ContactName;
                                                ContactCompany;
                                                Email;
                                                ContactAddress;
                                                ContactCity;
                                                ContactState;
                                                ContactPostal;
                                                ContactCountry;
                                            ]

                            if !valid then
                                let newAcc = {Api.Account.Empty with
                                                    Email = GetInputVal Email;
                                                    ContactName = GetInputVal ContactName;
                                                    ContactCompany = GetInputVal ContactCompany;
                                                    ContactAddress = GetInputVal ContactAddress;
                                                    ContactCity = GetInputVal ContactCity;
                                                    ContactState = GetInputVal ContactState;
                                                    ContactPostal = GetInputVal ContactPostal;
                                                    ContactCountry = GetInputVal ContactCountry;
                                                    CrmAccountId = "";
                                              }
                                let! res = Api.Admin.CreateAccount newAcc
                                match res with
                                | z when z.Success ->
                                    Success "Account created" parent
                                | z ->
                                    Error ("Unable to create account: "+ z.ErrorInfo + "(" + z.ErrorCode.ToString() + ")") parent
                      } 
                    )
                  ) |> ignore
                            
        //compose
        authScreen <|
            parent 
                -< [
                    H3[Attr.Class "blue"; Text "Create Account"; Attr.Style "margin-top:-6px;";]
                    ContactName;
                    ContactCompany;
                    Email;
                    Cl();
                    ContactAddress;
                    ContactCity;
                    ContactState;
                    ContactPostal;
                    ContactCountry;
                    Cl();
                    Br[]
                    saveBtn; 
                    msgHolder;
                    Cll();
                    ImpersonateControl();
                    Cll();
                    tableHolder;
                ]