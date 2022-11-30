module Demo911App.ApiKeys

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open Helpers
          
type ApiKeysControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let parent = Div[Attr.Class "well up6";] 
        //fields
        //let UserId = Dropdown "User" "Required" "" "input-medium" ["",""]
        //let Roles = Dropdown "Access" "Required" "" "input-medium" ["0","Read-only"; "1","Full";]
        let IPAddress = TextboxTip "IP Address" "Required" "input-medium" ""
        let IPMask = TextboxTip "IP Mask" "Required" "input-medium" ""
        let saveBtn = Button[Text "Create"; Attr.Class "btn btn-primary"; Attr.Style "margin-top:20px;";]
        let msgHolder = Div[]

        //get data
        let tableHolder = Div[]
        let keys = ref null
        let users = ref null
        
        Async.Start(
            async {
                let! res = Api.ListKeys (int Api.Vars.AccountId)
                if res.Success then keys := res.Data
                else ErrorAlert "Unable to load API keys"

                let! res2 = Api.ListUsers (int Api.Vars.AccountId)
                if res2.Success then users := res2.Data
                else ErrorAlert "Unable to load users"
                //User drop
                //let userDrop = JD(UserId.Dom.LastChild)
                //!users  |> Array.iter (fun (x: Api.User) -> userDrop.Append("<option value='"+(x.UserId.ToString())+"'>"+x.Name+"</option>") |> ignore)

                //keys table
                let getUserName id = try (!users |> Array.find(fun x -> x.UserId = id)).Name with x -> ""
                let rows =
                        !keys |> Array.map(fun x -> 
                                let dFun = fun () ->
                                    Async.Start(
                                        async {
                                            let! dRes = Api.DeleteKey(x.ApiKeyId)
                                            
                                            if dRes.Success then
                                                Success "API key deleted, reloading page..." parent
                                                refresh()
                                            else
                                                Error dRes.ErrorInfo parent
                                        }
                                    )

                                TR[
                                    //TD[Text x.IPAddress]
                                    //TD[Text <| x.IPMask.ToString()]
                                    //TD[Text <| match x.Roles with | z when z.ToString() = "1" -> "Full" | _ -> "Read-only"]
                                    //TD[Text <| getUserName (x.UserId)]
                                    TD[Text <| formatDate(x.CreatedOn)]
                                    TD[Text <| formatDate(x.ExpiresOn)]
                                    TD[ConfirmDeleteButton "Delete" dFun]
                                ]
                            )
                        |> Array.toSeq
                let table = Table[Attr.Class "table table-striped"] 
                            -< [TR[//TH[Text "IP Address"];TH[Text "IP Mask"]; TH[Text "Access"];
                                   TH[Text "Created"];TH[Text "Expired"];TH[];]]
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
                                                //IPAddress;
                                                //IPMask;
                                            ]                                            
                            let ipValid1 = true//ValidIp msgHolder IPAddress
                            let ipValid2 = true//ValidIp msgHolder IPMask

                            if !valid && ipValid1 && ipValid2 then
                                jsaveBtn.Attr("disabled","disabled") |> ignore
                                let newKey = Api.ApiKey.Empty //with
                                                    //UserId=int (GetInputVal UserId);
                                                    //Roles=match (GetInputVal Roles) with | z when z.ToString() = "1" -> Api.Roles.Owner | _ -> Api.Roles.Read;
                                                    //IPAddress=(GetInputVal IPAddress);
                                                    //IPMask=int (GetInputVal IPMask);
                                              //}
                                let! res = Api.CreateKey newKey
                                match res with
                                | z when z.Success ->
                                    let keyBox = 
                                        Div[Attr.Class "control-group info"; Attr.Style "width:100%;"] -<
                                            [
                                                Input[Attr.Class "size1 noPad noBorder noMar"; Attr.Style "width:100%; background-color:transparent;"; 
                                                      NewAttr "readonly" "readonly"; Attr.Value z.Data; ]
                                            ]
                                    let msgContent =
                                        Div[Attr.Class "size2 left"; Attr.Style "width:100%;"] -< [ 
                                            B[Text "New API Key created."; Attr.Class "size1"] 
                                            Br[]
                                            Br[]
                                            keyBox
                                            Br[]
                                            B[Text "Important: You must write this key down!"; Attr.Class "red"]
                                            Br[]
                                            Span[Text "Save this key right now as it won't be shown again."; Attr.Class "red"]
                                            Br[]
                                            Span[Text "We do not save API Keys and they cannot be viewed after creation."; Attr.Class "red"]
                                            Cl()
                                        ]
                                        //TODO highlighting JE(keyBox).Click(fun x _ -> JE(x).
                                    MessageAlert2 msgContent msgHolder "alert-info"
                                | z ->
                                    Error ("Unable to create API key: "+ z.ErrorInfo + "(" + z.ErrorCode.ToString() + ")") msgHolder
                                jsaveBtn.Delay(2000).RemoveAttr("disabled") |> ignore
                      } 
                    )
                  ) |> ignore
                            
        //compose
        authScreen <|
            parent 
                -< [
                    H3[Attr.Class "blue"; Text "API Keys"; Attr.Style "margin-top:-6px;";]
                    //IPAddress;
                    //IPMask;
                    //UserId;
                    //Roles;
                    Div[Attr.Class "left padT2 padR"] -< [ B[Text "Your Account ID is "]; Span[Text Api.Vars.AccountId; Attr.Class "blue";] ]
                    saveBtn;
                    msgHolder;
                    Cll();
                    tableHolder;       
                ]