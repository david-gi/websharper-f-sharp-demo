module Demo911App.Users

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open Helpers
          
type UsersControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let parent = Div[Attr.Class "well up6";] 
        //fields
        let Name = TextboxTip "Name" "Required" "input-medium" ""
        let Email = TextboxTip "Email" "Required" "input-medium" ""
        //let Roles = Dropdown "Access" "Required" "" "input-medium" ["0","Read-only"; "1","Full";]
        let IPAddress = TextboxTip "IP Address" "Required" "input-medium" ""
        let IPMask = TextboxTip "IP Mask" "Required" "input-medium" ""
        let newPass = TippedInput "Password" "Password must be at least 8 characters long" "password" "input-medium" ""
        let confPass = TippedInput "Confirm Password" "Must match new password" "password" "input-medium" ""
        let saveBtn = Button[Text "Create"; Attr.Class "btn btn-primary"; Attr.Style "margin-top:20px;";]
        let msgHolder = Div[]

        //get data
        let tableHolder = Div[]
        let users = ref null
        
        Async.Start(
            async {
                let! res = Api.ListUsers (int Api.Vars.AccountId)
                if res.Success then users := res.Data
                else ErrorAlert "Unable to load users"

                //table
                let rows =
                        !users |> Array.map(fun x -> 
                                let dFun = fun () ->
                                    Async.Start(
                                        async {
                                            let! dRes = Api.DeleteUser(x.UserId)
                                            
                                            if dRes.Success then
                                                Success "User deleted, reloading page..." parent
                                                refresh()
                                            else
                                                Error dRes.ErrorInfo parent
                                        }
                                    )

                                TR[
                                    TD[Text <| screenUndefined x.Name]
                                    TD[Text <| screenUndefined  x.Email]
                                    //TD[Text <| screenUndefined  x.IPAddress]
                                    //TD[Text <| screenUndefined(x.IPMask.ToString())]
                                    //TD[Text <| match x.Roles with | z when z.ToString() = "1" -> "Full" | _ -> "Read-only"]
                                    TD[ConfirmDeleteButton "Delete" dFun]
                                ]
                            )
                        |> Array.toSeq
                let table = Table[Attr.Class "table table-striped"] 
                            -< [TR[TH[Text "Name"];TH[Text "Email"];
                                //TH[Text "IP Address"];TH[Text "IP Mask"];TH[Text "Access"];
                                TH[];]]
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
                                                Name;
                                                Email;
                                                //IPAddress;
                                                //IPMask;
                                                //Roles;
                                                newPass;
                                            ]
                            let ipValid1 = true//ValidIp msgHolder IPAddress
                            let ipValid2 = true//ValidIp msgHolder IPMask
                                            
                            let newPassVal = GetInputVal newPass
                            let confPassVal = GetInputVal confPass
                            let passOk = ref true
                            if not(newPassVal = confPassVal) then Error "Passwords do not match" msgHolder; passOk := false
                            if newPassVal.Length < 8 then  Error "Password must be at least 8 characters long" msgHolder; passOk := false

                            if !valid && ipValid1 && ipValid2 && !passOk then
                                jsaveBtn.Attr("disabled","disabled") |> ignore
                                let newUser = {Api.User.Empty with
                                                    Name=GetInputVal Name;
                                                    Email=GetInputVal Email;
                                                    //Roles=match (GetInputVal Roles) with | z when z.ToString() = "1" -> Api.Roles.Owner | _ -> Api.Roles.Read;
                                                    //IPAddress=(GetInputVal IPAddress);
                                                    //IPMask=int (GetInputVal IPMask);
                                                }
                                let! res = Api.CreateUser newUser newPassVal
                                match res with
                                | z when z.Success ->
                                    Success "User created, reloading page..." msgHolder
                                    refresh()
                                | z ->
                                    Error ("Unable to create user: "+ z.ErrorInfo + "(" + z.ErrorCode.ToString() + ")") msgHolder
                                jsaveBtn.Delay(2000).RemoveAttr("disabled") |> ignore
                      } 
                    )
                  ) |> ignore
                            
        //compose
        authScreen <|
            parent 
                -< [
                    H3[Attr.Class "blue"; Text "Users"; Attr.Style "margin-top:-6px;";]
                    Name;
                    Email;
                    //IPAddress;
                    //IPMask;
                    //Roles;
                    //Cl();
                    newPass;
                    confPass;
                    saveBtn;
                    msgHolder;
                    Cll();
                    tableHolder;       
                ]