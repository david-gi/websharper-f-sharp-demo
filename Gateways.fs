module Demo911App.Gateways

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open Helpers
          
type GatewaysControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let parent = Div[Attr.Class "well up6";] 
        //fields
        let IP = TextboxTip "IP" "Required" "input-medium" ""
        let saveBtn = Button[Text "Create"; Attr.Class "btn btn-primary"; Attr.Style "margin-top:20px;";]
        let msgHolder = Div[]

        //get data
        let tableHolder = Div[]
        let gateways = ref null
        
        Async.Start(
            async {
                let! res = Api.ListGateways (int Api.Vars.AccountId)
                if res.Success then gateways := res.Data
                else ErrorAlert "Unable to load gateways"

                //table
                let rows =
                        !gateways |> Array.map(fun x -> 
                                let dFun = fun () ->
                                    Async.Start(
                                        async {
                                            let! dRes = Api.DeleteGateway(x.GatewayId)
                                            
                                            if dRes.Success then
                                                Success "Gateway deleted, reloading page..." parent
                                                refresh()
                                            else
                                                Error dRes.ErrorInfo parent
                                        }
                                    )

                                TR[
                                    TD[Text <| screenUndefined  x.IP]
                                    TD[Text <| formatDate(x.CreatedOn)]
                                    TD[ConfirmDeleteButton "Delete" dFun]
                                ]
                            )
                        |> Array.toSeq
                let table = Table[Attr.Class "table table-striped"] 
                            -< [TR[TH[Text "IP"];TH[Text "Created"];TH[];]]
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
                                                IP;
                                            ]
                            let ipValid = ValidIp msgHolder IP

                            if !valid && ipValid then
                                jsaveBtn.Attr("disabled","disabled") |> ignore
                                let gatewayIp = GetInputVal IP
                                let! res = Api.AddGateway gatewayIp (int Api.Vars.AccountId)
                                match res with
                                | z when z.Success ->
                                    Success "Gateway created, reloading page..." msgHolder
                                    refresh()
                                | z ->
                                    Error ("Unable to create gateway: "+ z.ErrorInfo + "(" + z.ErrorCode.ToString() + ")") msgHolder
                                jsaveBtn.Delay(2000).RemoveAttr("disabled") |> ignore
                      } 
                    )
                  ) |> ignore
                            
        //compose
        authScreen <|
            parent 
                -< [
                    H3[Attr.Class "blue"; Text "Gateways"; Attr.Style "margin-top:-6px;";]
                    IP;
                    saveBtn;
                    msgHolder;
                    Cll();
                    tableHolder;       
                ]