module Demo911App.Alerts

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open Helpers
          
type AlertsControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let subscriberName = Api.Vars.AlertName
        let number = Api.Vars.AlertNumber

        let parent = Div[Attr.Class "well up6";] 
        //fields
        let msgHolder = Div[]

        //get data
        let tableHolder = Div[]
        let alerts = ref Api.AlertList.Empty
        
        Async.Start(
            async {
                let! res = Api.GetAlertTargets(number)
                if res.Success then 
                    alerts := { Api.AlertList.Empty with
                                    AccountId=(box Api.Vars.AccountId :?> int);
                                    Targets= res.Data;
                              }                
                else ErrorAlert <| "Error loading Alert Contacts. " + res.ErrorInfo
                
                //table
                let buildRow (target: Api.AlertTarget) (isEmpty: bool) =
                        //js
                        let editFirstClickFun btn =
                                            let jRow = JD(btn).Parent().Parent()
                                            let toTextbox (colNum: int) =
                                                let jTd = jRow.Find("td:nth-child("+colNum.ToString()+")")
                                                let domInput = (Input[Attr.Class "input-small"; Attr.Style "width:120px;";Attr.Type "text"; Attr.Value <| (jTd.Text())]).Dom
                                                jTd.Html("").Append(domInput).Ignore
                                            ()
                                            toTextbox 1
                                            toTextbox 2
                                            let telInp = jRow.Find("td:nth-child(2) :input")
                                            telInp.Keyup(fun _ k ->
                                                            if screenKeys(k.Which) then
                                                                maskNumberInput telInp
                                                        ) |> ignore
                                            toTextbox 4
                                            let smsInp = jRow.Find("td:nth-child(4) :input")
                                            smsInp.Keyup(fun _ k -> 
                                                            if screenKeys(k.Which) then
                                                                maskNumberInput smsInp
                                                        ) |> ignore

                                            toTextbox 5
                                            //toTextbox 7
                                            let toCheckbox (colNum: int) =
                                                let jTd = jRow.Find("td:nth-child("+colNum.ToString()+")")
                                                let domInput =
                                                        let btn = Button[Text "Yes"; Attr.Class "btn-success btn-small";]
                                                        let isSnd = ref false;
                                                        let jbtn = JE(btn)
                                                        jbtn.Click(fun e o ->
                                                                if !isSnd then
                                                                    jbtn.RemoveClass("btn-danger").AddClass("btn-success").Text("Yes") |> ignore
                                                                    isSnd := false
                                                                else
                                                                    isSnd := true
                                                                    jbtn.RemoveClass("btn-success").AddClass("btn-danger").Text("No") |> ignore
                                                            ) |> ignore
                                                        if jTd.Text() = "No" then 
                                                            isSnd := true
                                                            jbtn.RemoveClass("btn-success").AddClass("btn-danger").Text("No") |> ignore
                                                        (btn).Dom
                                                jTd.Html("").Append(domInput).Ignore
                                                    
                                            toCheckbox 3
                                            toCheckbox 6
                                            ()     

                        let editSecondClickFun alertTargetId btn =
                                            let jRow = JD(btn).Parent().Parent()
                                            let getText (colNum: int) =
                                                let jTb = jRow.Find("td:nth-child("+colNum.ToString()+") :input")
                                                let text = ref (jTb.Val() :?> string)
                                                if colNum = 2 || colNum = 4 then text := (cleanNumber !text); if not(IsNullEmptyOrUndefined !text) && not((!text).StartsWith("1")) then text := "1" + !text
                                                if IsNullEmptyOrUndefined ((!text).Trim()) then "" else !text
                                            let toText (colNum: int) =
                                                let jTb = jRow.Find("td:nth-child("+colNum.ToString()+") :input")
                                                let text = ref (jTb.Val() :?> string)
                                                if colNum = 2 || colNum = 4 then 
                                                    if not(IsNullEmptyOrUndefined !text) && not((!text).StartsWith("1")) then text := "1" + !text
                                                    text := maskNumber(!text)
                                                jTb.Parent().Html(!text) |> ignore
                                            let getYesOrNo (colNum: int) =
                                                let jEl = jRow.Find("td:nth-child("+colNum.ToString()+") :button")
                                                if jEl.Text() = "Yes" then true else false
                                            let toYesOrNo (colNum: int) =
                                                let jEl = jRow.Find("td:nth-child("+colNum.ToString()+") :button")
                                                jEl.Parent().Html(if jEl.Text() = "Yes" then "Yes" else "No").Ignore
                                            
                                            //API call
                                            let aTarget = { Api.AlertTarget.Empty with 
                                                              AlertTargetId = alertTargetId;
                                                              Name = ""+getText 1;
                                                              Phone = ""+getText 2;
                                                              AllowStreaming = getYesOrNo 3;
                                                              Sms = ""+getText 4;
                                                              Email = ""+getText 5;
                                                              AttachRecording = getYesOrNo 6;
                                                              //HttpCallback = getText 7;
                                                          }
                                                          
                                            Async.Start(
                                                async {
                                                        let! res = Api.SetAlertTarget(aTarget, number)
                                                        if not res.Success then ErrorAlert res.ErrorInfo;
                                                        else Success "Alert Contact saved!" parent
                                                             toText 1
                                                             toText 2
                                                             toText 4
                                                             toText 5
                                                             //toText 7
                                                             toYesOrNo 3
                                                             toYesOrNo 6
                                                }
                                            )          
                                            Async.Start(
                                                async {
                                                    try
                                                        let! resx = Api.DeleteAlertTarget(number, alertTargetId)
                                                        ()
                                                    with x -> ()
                                                }
                                            )
                                            refresh()                                  

                        let deleteFun (alertTargetId: int) (element: Dom.Element) =
                                            Async.Start(
                                                async {
                                                    let! res = Api.DeleteAlertTarget(number, alertTargetId)
                                                    if not res.Success then ErrorAlert res.ErrorInfo;
                                                    else 
                                                        Success "Alert Contact removed." parent
                                                        JD(element).Remove().Ignore
                                                }
                                            )
                                            ()

                        let row = TR[
                                    TD[Text <| screenUndefined target.Name ]
                                    TD[Text <| screenUndefined  target.Phone ]
                                    TD[Text <| if isEmpty then "" else if target.AllowStreaming then "Yes" else "No" ]
                                    TD[Text <| screenUndefined  target.Sms ]
                                    TD[Text <| screenUndefined  target.Email ]
                                    TD[Text <| if isEmpty then "" else if target.AttachRecording then "Yes" else "No" ]
                                    //TD[Text <| screenUndefined  target.HttpCallback ]
                                    TD[
                                         EditSaveButton (if isEmpty then "Add" else "Edit") editFirstClickFun (editSecondClickFun target.AlertTargetId)
                                         Span[Text " "; Attr.Style "padding:0 6px;"]
                                         ConfirmDeleteButton2 "Remove" (deleteFun target.AlertTargetId)
                                      ]
                                 ]
                        row

                let dRows = if alerts.Value.Targets = null || alerts.Value.Targets.Length  < 1 then [| Span[] |]
                            else alerts.Value.Targets |> Array.map(fun x -> buildRow x false)

                let addNew () =
                    let aTable = JS("#AlertsContactTable")
                    let aRowLength = if (alerts.Value.Targets = null || alerts.Value.Targets.Length  < 1) && aTable = JavaScript.Undefined then 0 else aTable.Find("tr").Length
                    if aRowLength < 6 then
                        let emptyRow = buildRow (Api.AlertTarget.Empty) false
                        aTable.Append(emptyRow.Dom).Ignore
                        JE(emptyRow).Find("button:first-child").Click().Ignore
                    else
                        let maxMsg = Span[Attr.Class "gray size-1 padL"; Text "Max amount of Alert Contacts reached."] 
                        JS("#AddAlertTargetBtn").After(maxMsg.Dom).Ignore
                        JE(maxMsg).Delay(2000).FadeOut().Ignore
                                                                    
                let table = Table[Attr.Class "table table-striped"; Attr.Id "AlertsContactTable";] 
                            -< [TR[
                                    TH[Abbr[Text "Nickname"; Attr.Title"Provide an easy to remember nickname for this contact"]];
                                    TH[Abbr[Text "Phone"; Attr.Title"Provide a phone number for voice alerts, and live 9-1-1 listening"]];
                                    TH[Abbr[Text "ListenIN?"; Attr.Title"If yes, the alert contact will be allowed to listen in to the 9-1-1 call live, and communicate with other alert contacts via a conference bridge"]];
                                    TH[Abbr[Text "SMS"; Attr.Title"Provide a cell phone number for text alerts"]];
                                    TH[Abbr[Text "Email"; Attr.Title"Provide an email for email alerts"]];
                                    TH[Abbr[Text "Gets Recording?"; Attr.Title"If yes, the email provided will receive a link to the 9-1-1 recording when the call ends"]];
                                    TH[];
                               ]]
                            -< dRows
                let addBtn = Button[Text "Add a new Alert Contact"; Attr.Class "btn btn-success marB marT"; Attr.Id "AddAlertTargetBtn"]
                JE(tableHolder).Append(addBtn.Dom) |> ignore
                JE(addBtn).Click(fun e x -> addNew() |> ignore).Ignore
                JE(tableHolder).Append(table.Dom) |> ignore
            }
        )
        
                            
        //compose
        authScreen <|
            parent 
                -< [
                    H3[Attr.Class "blue"; Text ("Alerts for: "+subscriberName + " - " + maskNumber(number)); Attr.Style "margin-top:-6px;";]
                    
                    Div[Attr.Class "offset1"]
                        -< [
                            Span[Attr.Class "size3 padL2 bold purple"; Text "Revolutionary new safety feature!"]
                            Br[]
                            Span[Attr.Class "size1 padL2 "; Text "ListenIN allows people you authorize to hear your call to 9-1-1 in real-time."]
                            Br[]
                            Span[Attr.Class "size1 padL2 "; Text "They can speak to each other and help coordinate immediate safety measures."]
                        ]
                    Br[]
                    Br[]
                    Div[]
                        -< [
                            B[Attr.Class "blue size2 marL2"; Attr.Style "padding:0 0 6px 6px;"; Text "For example: "]
                            Br[]
                            Div[Attr.Class "alert alert-info size1 black span8"; Attr.Style "line-height:1.6em";]
                                -< [
                                    Span[Text "Let’s say that Alice has this app configured on her office phone in office 721."]
                                    Span[Text "If Alice dials 9-1-1, this app will immediately send out Alerts using text, voice and email to the people Alice has provisioned in her this app profile."]
                                    Br[]
                                    Br[]
                                    Span[Text "Alice has listed the lobby security guard, Bob, and her office manager, Carol, as Alert contacts with "]
                                    I[Text "ListenIN"]
                                    Span[Text " enabled. When Alice dials 9-1-1, in addition to SMS and email both Bob and Carol  will receive a telephone call announcing that “A call to 9-1-1 was placed by Alice. To listen in, press 1”. Immediately, Bob and Carol  will be joined into a conference bridge where they will be able to hear Alice’s live, in-progress 9-1-1 call. And, while listening to the live 9-1-1 call, Bob and Carol will be able to hear and talk to each other and plan an emergency response strategy in real time."]
                                    Br[]
                                    Br[]
                                    Span[Text "This allows Bob and Carol to immediately be aware of Alice’s specific situation. They can then prepare for police or EMTs and plan an emergency response strategy in real time."]
                                    Br[]
                                    Br[]
                                    B[Text "this app increases the safety and response time for all 9-1-1 emergency calls."]

                                ]
                            Cll()
                        ]
                    msgHolder;
                    H4[Attr.Class "blue"; Text "Alert Contacts";]
                    B[Text "You can have up to five Alert Contacts."]
                    tableHolder;
                    Cll()
                ]