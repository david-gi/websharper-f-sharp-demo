module Demo911App.Subscribers

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open Helpers
  
type SubscriberControl() = 
    inherit Control()

    ///Address add/edit form
    [<JavaScript>]
    let buildSubsForm (subs : Api.Subscriber, alertsDisabled) = 
        let isNew = subs.AccountId = -1
        let addr =  subs.Address
        //subscriber fields
        let parent = Div[Attr.Class "well up6";] 
        let number = H3[Text <| "1" + (maskNumber subs.Number); Attr.Class "blue"; Attr.Style " margin-top:-6px;"]
        
        let detailsCreated = Span[Text (screenUndefined (formatDate <| subs.CreatedOn)); Attr.Class "marL"]
        let detailsUpdated = Span[Text (screenUndefined (formatDate subs.LastUpdated)); Attr.Class "marL"]
        let hasAlt = subs.AddressStatus.AddressCode = 1 
        let altAddrStr = if hasAlt then buildAddrStr subs.AddressStatus.Alternatives else ""
        let detailsAlt = Span[Text altAddrStr; Attr.Class <| if not hasAlt then "hide" else "" ]
        
        let subsDetails =    
            Div[Attr.Class ("alert alert-info padB" + (if isNew then " hide" else ""))] -<
            [   
                Span[Text (if IsNullEmptyOrUndefined <| subs.DeletedOn.ToString() then "" else " DELETED "); Attr.Class "red bold "]
                Strong[Text "Created"]
                detailsCreated
                Strong[Text "Last Updated"; Attr.Class "marL2"]
                detailsUpdated
                Br[]
                Strong[Text "Saved alternative address"; Attr.Class "marR"; Attr.Class <| if not hasAlt then "hide" else "" ]
                detailsAlt 
            ]

        //address fields
        let SubscriberName = TextboxTip2 "Name" "Name of telephone number owner" "Business or Person" "input-xlarge required" (screenUndefined addr.SubscriberName)
        let GroupName = TextboxTip2 "Group" "Optional, used to group addresses for internal management." "" "" (screenUndefined subs.GroupPath)
        let HouseNumber = TextboxTip2 "Street Number" "Street or house number" "Ex: 707" "input-small required" addr.HouseNumber
        let HouseNumberSuffix = TextboxTip2 "Suffix" "Additional part of street number" "Ex: 1/2" "input-mini"  addr.HouseNumberSuffix
        let roadStr =  ref <| "" +
                        (if IsNullEmptyOrUndefined addr.RoadPreDir then "" else addr.RoadPreDir + " ") +
                        screenUndefined addr.Road + " " +
                        (if IsNullEmptyOrUndefined addr.RoadSuffix then "" else addr.RoadSuffix + " ") +
                        (if IsNullEmptyOrUndefined addr.RoadPostDir then "" else addr.RoadPostDir)
        if (!roadStr).Length < 2 then roadStr := ""

        let Road = TextboxTip2 "Street Name" "Must contain directionals and abbreviations" "Ex: W Hampden Ave" "input-xlarger required" !roadStr
        let Location = TextboxTip2 "Location" "Additional location information" "Ex: Bldg 9 Flr 3 Room 301" "" addr.Location
        let Community = TextboxTip2 "City" "City, town or community name" "" "input-medium required" addr.Community
        let State = TextboxTip2 "State" "2-letter state code" "" "input-mini required" addr.State
        let Country = Dropdown "Country" "Currently only United States is allowed" addr.Country "required" ["US","United States";"CA","Canada";]
        let PostalCode = TextboxTip2 "ZIP/Postal Code" "ZIP or postal code" "" "input-small required" addr.PostalCode 
        let saveBtn = Button[Text "Save"; Attr.Class "btn btn-primary"]
        let cancelBtn = Button [Attr.Class "btn marL"; Text "Cancel"]
        let alertLink = A[Attr.Class <| "right btn pointer marR"+ (if isNew || alertsDisabled then " hide" else "");  Text "Setup Alerts"]
        let deleteBtn = Button [Attr.Class <| "btn btn-danger right" + (if (isNew || not (IsNullEmptyOrUndefined <| subs.DeletedOn.ToString())) then " hide" else ""); Attr.Style "margin-left:80px;"; Text "Delete Address"]
        let testToggle = if subs.NextTestSendsAlerts then Button[Text "Yes"; Attr.Class <| "btn-warning right btn-small marR" + (if isNew || alertsDisabled then " hide" else "");]
                         else Button[Text "No"; Attr.Class <| "btn-small right btn-info marR" + (if isNew || alertsDisabled then " hide" else "");]
                         
        //input filtering
        JD(SubscriberName.Dom.LastChild).Attr("maxlength", "32").Ignore
        JD(GroupName.Dom.LastChild).Attr("maxlength", "32").Ignore
        JD(HouseNumber.Dom.LastChild).Attr("maxlength", "10").Ignore
        JD(HouseNumberSuffix.Dom.LastChild).Attr("maxlength", "4").Ignore
        JD(State.Dom.LastChild).Attr("maxlength", "2").Ignore
        JD(Road.Dom.LastChild).Attr("maxlength", "60").Ignore
        JD(Community.Dom.LastChild).Attr("maxlength", "32").Ignore
        JD(State.Dom.LastChild).Attr("maxlength", "").Ignore
        JD(PostalCode.Dom.LastChild).Attr("maxlength", "10").Ignore
        JD(Location.Dom.LastChild).Attr("maxlength", "20").Ignore

        //js
        JE(alertLink).Click(fun e x ->
            Api.Vars.SetAlertName subs.Address.SubscriberName
            Api.Vars.SetAlertNumber subs.Number
            Html5.Window.Self.Location.Replace "/portal/Alerts").Ignore

            
        let jtestToggle = JE(testToggle);
        jtestToggle.Click(fun e o ->
                Async.Start(
                    async {
                        let isOn = jtestToggle.Text() = "Yes";
                        let! res = Api.SetNextTestSendAlerts subs.Number
                        match res with
                        | z when z.Success ->
                            if isOn then
                                jtestToggle.AddClass("btn-info").RemoveClass("btn-warning").Text("No").Ignore
                            else
                                jtestToggle.AddClass("btn-warning").RemoveClass("btn-info").Text("Yes").Ignore
                        | z ->
                            Error ("Unable change alert testing mode. "+ z.ErrorInfo) parent
                    })
            ) |> ignore

        JD(Country.Dom.LastChild).Click(fun x _ -> J(x).Val("US") |> ignore) |> ignore
        let jsaveBtn = JE(saveBtn)
        jsaveBtn.Click(fun x _ -> 
                    Async.Start(
                        async {
                            ClearMessageAlerts() 
                            
                            //validate
                            let valid = Required parent
                                            [
                                                SubscriberName
                                                HouseNumber
                                                Road
                                                Community
                                                State
                                                Country
                                                PostalCode
                                            ]

                            if !valid then
                                jsaveBtn.Attr("disabled","disabled") |> ignore
                                                        
                                let newAddr = {subs.Address with
                                                 SubscriberName=GetInputVal SubscriberName;
                                                 Community=GetInputVal Community; 
                                                 Country=GetInputVal Country; 
                                                 State=GetInputVal State; 
                                                 HouseNumber=GetInputVal HouseNumber;
                                                 HouseNumberSuffix=GetInputVal HouseNumberSuffix; 
                                                 Location=GetInputVal Location;
                                                 PostalCode=GetInputVal PostalCode; 
                                                 Number=subs.Number; 
                                                 Road=GetInputVal Road;
                                                 RoadPostDir="";
                                                 RoadPreDir="";
                                                 RoadSuffix="";
                                              }
                                let newSubs = { subs with Api.Subscriber.Address = newAddr; Api.Subscriber.GroupPath = GetInputVal GroupName; }
                                
                                let! res = Api.SetSubscriber newSubs
                                
                                let setAltFun (altAddr: Api.Address) =
                                    SetInputVal Community altAddr.Community |> ignore
                                    SetInputVal State altAddr.State |> ignore
                                    SetInputVal Country altAddr.Country |> ignore
                                    SetInputVal HouseNumber altAddr.HouseNumber |> ignore
                                    SetInputVal HouseNumberSuffix altAddr.HouseNumberSuffix |> ignore
                                    if not (IsNullEmptyOrUndefined altAddr.Location) then SetInputVal Location altAddr.Location |> ignore
                                    SetInputVal PostalCode altAddr.PostalCode |> ignore
                                    SetInputVal Road altAddr.Road |> ignore
                                    jsaveBtn.Click() |> ignore

                                let alertAltAddrsFun = fun (altAddr: Api.Address) ->
                                                    let altBox = Div[]
                                                        
                                                    let altBtn = Button[Attr.Class "btn btn-warning marT"; Text "Populate above fields with this address instead?";]
                                                    JE(altBtn).Click(fun _ _ -> setAltFun altAddr;) |>ignore
                                                    MessageAlert2 
                                                                (altBox -< [
                                                                    Span[Text "An alternative address was suggested:"; Attr.Style "font-size:1.2em;"]
                                                                    Br[]
                                                                    B[Text <| buildAddrStr([|altAddr|]) ]
                                                                    Br[]
                                                                    altBtn
                                                                ]) parent "alert-warning"

                                jsaveBtn.Delay(2000).RemoveAttr("disabled") |> ignore
                                match res with
                                | z when z.Success ->

                                    match z.Data with
                                    | s when s.AddressCode = 0 -> 
                                        Success "Success! This address has been saved." parent
                                        JE(alertLink).Show() |> ignore
                                        JE(deleteBtn).Show() |> ignore
                                    | s when s.AddressCode = 1 -> 
                                        Success "Success! This address has been saved." parent
                                        //show alt add box
                                        let altAddrs = s.Alternatives
                                        altAddrs |> Array.iter(alertAltAddrsFun)
                                        JE(alertLink).Show() |> ignore
                                        JE(deleteBtn).Show() |> ignore                       
                                    | s when s.AddressCode = 2 -> 
                                        Error ("Warning, address has been saved but with errors: " + s.Message) parent
                                        //show alt add box
                                        if (not(s.Alternatives = null) && s.Alternatives.Length > 0) then
                                            let altAddrs = s.Alternatives
                                            altAddrs |> Array.iter(alertAltAddrsFun)
                                    | s -> 
                                        Error ("Unable to save address, internal error: " + s.Message) parent
                                | z ->
                                    Error ("Unable to save address: "+ z.ErrorInfo) parent
                                
                      } 
                    )
                  ) |> ignore
        JE(cancelBtn).Click(fun x _ -> J(x).Parent().Remove() |> ignore) |> ignore
        let jdeleteBtn = JE(deleteBtn)
        let sndClick = ref false
        jdeleteBtn.Click(fun x _ -> 
            Async.Start( async{ 
                    sndClick := true
                    jdeleteBtn.RemoveClass("btn-danger").AddClass("btn-warning").Text("Double-click to Confirm") |> ignore 
                    JavaScript.SetTimeout(fun() -> sndClick := false; jdeleteBtn.RemoveClass("btn-warning").AddClass("btn-danger").Text("Delete").Ignore) |> ignore
            })) |> ignore
        jdeleteBtn.Dblclick(fun x _ -> 
            Async.Start( async{ 
                if(!sndClick) then 
                    jdeleteBtn.RemoveClass("btn-warning").Hide() |> ignore                     
                    sndClick := false
                    let! dres = Api.DeleteSubscriber subs.Number 
                    Warning "This address been deleted." parent
            })) |> ignore
        ()

        Div[Attr.Class ""]
            -< [
                //address form
                parent -<
                [  
                    number
                    subsDetails
                    SubscriberName
                    GroupName
                    Cll()
                    HouseNumber
                    HouseNumberSuffix
                    Cll()
                    //RoadPreDir
                    Road
                    //RoadSuffix
                    //RoadPostDir
                    Location
                    Cll()
                    Community
                    State
                    Country
                    PostalCode                    
                    Cll()
                    Div[Attr.Class "left"] -< [
                        testToggle
                        Abbr[Attr.Class <| "size1 bold right attribute marR" + (if isNew || alertsDisabled then " hide" else "");
                        Attr.Style "margin-top:4px;"; Attr.Title "Send alerts on the next test call(933)."; Text "Test Alerts?"] -< [
                            I[ Attr.Class "icon-info-sign faded"; Attr.Style "margin-left:2px;"]
                        ]
                        alertLink
                    ]
                    Br[]
                    Cll()
                    saveBtn
                    cancelBtn
                    deleteBtn
                    Br[]
                ]
            ]

    [<JavaScript>]
    override this.Body =  
        let content = 
            let hash = Window.Self.Location.Hash;

            //create
            let parent = Div[ Attr.Class "control-group success"]
            let numTB = Input[Id "numTB"; Attr.Type "Text"; NewAttr "placeholder" "Enter a new or existing number..."; NewAttr "data-mask" "(999) 999-9999";
                              Attr.Class "input-xlarge prependedInput size-2 bold green"; Attr.Style "padding:9px; margin:0;"]
            let btn = Button[ Id "searchBtn"; Text "Go"; Attr.Class "btn btn-success"; Attr.Style "height:40px; margin-left:4px; margin-bottom:2px;"; NewAttr "data-loading-text" "Saving..." ]
            let errorMsg = InputHelp ""
            let subsArea = Div[]
                      
            //js
            let jBtn = JE(btn)
            let jnumTB = JE(numTB)
            catchEnterKey jnumTB jBtn
            jnumTB.Keyup(fun _ k -> 
                    if screenKeys(k.Which) then
                        maskNumberInput jnumTB
                ) |> ignore
            jBtn.Click(fun _ _ -> 
                          JE(subsArea).Html "" |> ignore
                          let jparent = JE(parent)
                          jparent.RemoveClass("success").RemoveClass("error") |> ignore
                          let jerrormsg = JE(errorMsg)
                          jerrormsg.Hide() |> ignore
                          Async.Start(
                              async {
                                 let! areAlertsEnabled = Api.AreAlertsEnabled()
                                 let alertsDisabled = not(areAlertsEnabled.Data)
                                 let cleanNum = "1" + (cleanNumber numTB.Value)
                                 let! subs = Api.GetSubscriber cleanNum
                                 let clearNumTB () = 
                                     let jnumTB = JE(numTB)
                                     jnumTB.Val("").Ignore
                                
                                 match subs with
                                 | x when x.Success -> 
                                     if not(IsNullEmptyOrUndefined <| x.Data.DeletedOn.ToString()) then
                                         subsArea -< [buildSubsForm({ Api.Subscriber.Empty with Number=cleanNum }, alertsDisabled)] |> ignore
                                         jparent.AddClass("success").Delay(5000).RemoveClass("success") |> ignore
                                         jerrormsg.Text "Found Address" |> ignore
                                         jerrormsg.FadeIn().Delay(5000).FadeOut() |> ignore
                                         clearNumTB()
                                     else
                                         subsArea -< [buildSubsForm(x.Data, alertsDisabled)] |> ignore
                                         jparent.AddClass("success").Delay(5000).RemoveClass("success") |> ignore
                                         jerrormsg.Text "Found Address" |> ignore
                                         jerrormsg.FadeIn().Delay(5000).FadeOut() |> ignore
                                     clearNumTB()
                                 | x when (not x.Success) && (x.ErrorCode = 7) -> 
                                     subsArea -< [buildSubsForm({ Api.Subscriber.Empty with Number=cleanNum}, alertsDisabled)] |> ignore
                                     jparent.AddClass("success").Delay(5000).RemoveClass("success") |> ignore
                                     jerrormsg.Text "New Address" |> ignore
                                     jerrormsg.FadeIn().Delay(5000).FadeOut() |> ignore
                                     clearNumTB()
                                 | x  -> 
                                     jparent.AddClass("error").Delay(5000).RemoveClass("error") |> ignore
                                     if x.ErrorInfo.Contains "Subscriber owned by another account." then
                                         jerrormsg.Text "That number is owned by another account." |> ignore
                                     else
                                         jerrormsg.Text x.ErrorInfo  |> ignore
                                     jerrormsg.Show() |> ignore
                                 JE(subsArea).FadeIn() |> ignore
                              }
                          ) 
                      ) |> ignore
            JQuery.JQuery.Of("document").Ready(fun _ ->
                    jnumTB.Focus() |> ignore 
                    if not(IsNullEmptyOrUndefined hash) then
                        numTB.Value <- hash.Substring(1)
                        JE(btn).Click().Ignore
                ) |> ignore
            //compose            
            parent
                -< [
                    Span [Attr.Class "input-prepend"] -<
                    [
                        Span [ Text "+1"; Attr.Class "add-on btn-success"; Attr.Style "color:#fff;padding:9px;"]
                        numTB
                    ]
                    btn
                    A[Html.Default.Attr.Style "margin-left:6px;"; Text "Bulk Upload"; Attr.HRef "/portal/BulkUpload"]
                    errorMsg
                    Br[]
                    Br[]
                    subsArea
                ]
        
        authScreen <| 
                Div [
                    H3[Html.Default.Text "Addresses"; Html.Default.Attr.Style "margin-top:-6px;"; Html.Default.Attr.Class "blue";] 
                    B [Text "Add or Update an Address";]; 
                    content
                ]
