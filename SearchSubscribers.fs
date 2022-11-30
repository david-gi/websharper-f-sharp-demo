module Demo911App.SearchSubscribers

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open Helpers
          
type SearchSubscribersControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let parent = Div[Attr.Class "well up6";] 
        //fields
        let Name = TextboxTip "Name" "Name of telephone number owner" "input-large" ""
        let Tel = TextboxTip "Number" "Full or part of telephone number" "input-medium" ""
        let AddrTb = TextboxTip "Address" "Full or part of the address" "input-xlarge" ""
        let btn = Button[Text "Search"; Attr.Class "btn btn-primary"; Attr.Style "margin-top:20px;";]
        let dlBtn = Button[Text "Download Complete List of Addresses"; Attr.Class "btn  marL"; Attr.Style "margin-top:20px;";]
        let prevBtn = Button[Text "Previous"; Attr.Class "btn faded"; Attr.Style "width:90px;"; ]
        let nextBtn = Button[Text "Next"; Attr.Class "btn"; Attr.Style "width:90px;" ]
        let pageNum = Span[Attr.Class "bold gray padL padR"]
        let msgHolder = Div[]

        //get data
        let tableHolder = Div[]
        let addrs = ref null
        let size = ref 15;
        let page = ref 0;

        let Search () =
            Async.Start(
                async {
                    JE(pageNum).Text("page "+(!page + 1).ToString()).Ignore
                    let nameV = (JE(Name).Find("input").Val()  :?> string)
                    let numV = (JE(Tel).Find("input").Val()  :?> string).Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")
                    let addrV = (JE(AddrTb).Find("input").Val()  :?> string)
                    
                    let! res = Api.SearchSubscribers ((int Api.Vars.AccountId), nameV, numV, addrV, !page, !size)
                    if res.Success then addrs := res.Data
                    else ErrorAlert "Unable to load addresses"
                    if res.Data.Length < !size then JE(nextBtn).AddClass("faded").Ignore
                    else JE(nextBtn).RemoveClass("faded").Ignore
                    if !page < 1 then JE(prevBtn).AddClass("faded").Ignore
                    else JE(prevBtn).RemoveClass("faded").Ignore

                    //table
                    let rows =
                            !addrs |> Array.map(fun x -> 
                                    let lBtn = Button[Text "Details"; Attr.Class "btn btn-info marR";]
                                    JE(lBtn).Click(fun e o ->
                                                    Html5.Window.Self.Location.Replace(Html5.Window.Self.Location.Pathname + "#" + x.Number.Substring(1))
                                                    refresh()
                                                ).Ignore

                                    let dFun = fun () ->
                                        Async.Start(
                                            async {
                                                let! dRes = Api.DeleteSubscriber(x.Number)
                                            
                                                if dRes.Success then
                                                    Success "Address deleting, reloading page..." parent
                                                    refresh()
                                                else
                                                    Error dRes.ErrorInfo parent
                                            }
                                        )

                                    TR[
                                        TD[Text <| screenUndefined x.Address.SubscriberName]
                                        TD[Text <| maskNumber(screenUndefined  x.Number)]
                                        TD[Text <| buildAddrStr([|x.Address|])]
                                        TD[ lBtn; ]//ConfirmDeleteButton "Delete" dFun;]
                                    ]
                                )
                            |> Array.toSeq
                    let table = Table[Attr.Class "table table-striped"] 
                                -< [TR[TH[Text "Name"];TH[Text "Number"];TH[Text "Address"];TH[];]]
                                -< rows
                    JE(tableHolder).Append(table.Dom) |> ignore
                }
            )
        Search ()
        
        //js       
        let reSearch (index: int32) =
            JE(tableHolder).Children().Remove().Ignore
            page := index
            Search ()
        JE(prevBtn).Click(fun e k -> if not(JD(e).Attr("class").Contains("faded")) then 
                                        let p = (!page - 1); 
                                        reSearch <| (if(p < 0) then 0 else p)).Ignore
        JE(nextBtn).Click(fun e k -> if not(JD(e).Attr("class").Contains("faded")) then reSearch (!page + 1)).Ignore
        
        JE(Tel).Keyup(fun e k -> 
                    if screenKeys(k.Which) then
                        maskNumberInput <| JD(e)
                ).Ignore
        JE(btn).Click(fun e k -> 
                    reSearch 0
                ).Ignore
        catchEnterKey (JE(Tel)) (JE(btn))
        catchEnterKey (JE(Name)) (JE(btn))
        catchEnterKey (JE(AddrTb)) (JE(btn))
        JE(dlBtn).Click(fun e k ->                     
                    Async.Start(
                        async {
                            let! dlRes = Api.EmailAllSubscribersAsAttachment(int Api.Vars.AccountId)                                            
                            if dlRes.Success then
                                InfoAlert "Due to the file size, a complete address list will be sent to you by email within the next few minutes."
                            else
                                Error dlRes.ErrorInfo parent
                        }
                    )
                ).Ignore        
                            
        //compose
        authScreen <|
            parent 
                -< [
                    H3[Attr.Class "blue"; Text "Search Addresses"; Attr.Style "margin-top:-6px;";]
                    Name;
                    Tel;
                    AddrTb;
                    btn;
                    dlBtn;
                    msgHolder;
                    Cll();
                    tableHolder;
                    Div[Attr.Class "centerTxt"] -<[
                        prevBtn
                        pageNum
                        nextBtn
                    ]    
                ]