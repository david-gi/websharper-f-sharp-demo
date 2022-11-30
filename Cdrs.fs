module Demo911App.Cdrs

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open Helpers
          
type CdrsControl() = 
    inherit Control()    
        
    [<JavaScript>]
    override this.Body = 
        let parent = Div[Attr.Class "well up6";] 

        //fields
        let Tel = TextboxTip "Number" "Full or first part of telephone number" "input-medium" ""
        let SrcIP = TextboxTip "Source IP" "" "input-medium" ""
        let DstIP = TextboxTip "Destination IP" "" "input-medium" ""
        let SipCode = TextboxTip "SIP Code" "" "input-medium" ""
        let Start = TextboxTip "From Date" "dd/mm/yyyy" "input-medium" ""
        let End = TextboxTip "To Date" "dd/mm/yyyy" "input-medium" ""

        let btn = Button[Text "Search"; Attr.Class "btn btn-primary"; Attr.Style "margin-top:20px;";]
        let dlBtn = Button[Text "Download All Results"; Attr.Class "btn  marL"; Attr.Style "margin-top:20px;";]
        let prevBtn = Button[Text "Previous"; Attr.Class "btn faded"; Attr.Style "width:90px;"; ]
        let nextBtn = Button[Text "Next"; Attr.Class "btn"; Attr.Style "width:90px;" ]
        let pageNum = Span[Attr.Class "bold gray padL padR"]
        let msgHolder = Div[]

        //get data
        let tableHolder = Div[]
        let cdrs = ref null
        let size = ref 50;
        let page = ref 0;

        let bindCrit() =
                    let TelV = (JE(Tel).Find("input").Val()  :?> string).Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")
                    let SrcIPV = (JE(SrcIP).Find("input").Val()  :?> string)
                    let SipCodeV = (JE(SipCode).Find("input").Val()  :?> string)
                    let StartV = (JE(Start).Find("input").Val()  :?> string)
                    let EndV = (JE(End).Find("input").Val()  :?> string)
                    { Api.CdrSearchCriteria.Empty with Number=TelV; SrcIP=SrcIPV; SipCode=SipCodeV; CreatedOnStart=StartV; CreatedOnEnd=EndV; Index= !page; Size= !size; }


        let Search () =
            Async.Start(
                async {
                    JE(pageNum).Text("page "+(!page + 1).ToString()).Ignore

                    let! res = Api.GetCdrs <| bindCrit()
                    if res.Success then cdrs := res.Data
                    else 
                        ErrorAlert "Unable to load CDRs"
                        return ()
                    if res.Data.Length < !size then JE(nextBtn).AddClass("faded").Ignore
                    else JE(nextBtn).RemoveClass("faded").Ignore
                    if !page < 1 then JE(prevBtn).AddClass("faded").Ignore
                    else JE(prevBtn).RemoveClass("faded").Ignore

                    //table
                    let table = Table[Attr.Class "table table-striped"] 
                                -< [TR[TH[];TH[Text "Number"];TH[Text "Src IP"];TH[Text "Code"];TH[Text "Service"];TH[Text "Answered"];TH[Text "Duration"];]]
                    !cdrs |> Array.iter(fun x -> 
                            let unprovisioned = IsNullEmptyOrUndefined <| x.AddressId.ToString()
                            let numberString = " " + maskNumber(screenUndefined  x.Number);
                            let row = TR[Attr.Class <| "pointer ";] 
                                        -< [
                                            TD[
                                                B[Text "+"; Attr.Class <| ("plus rounded ")];
                                            ]
                                            TD[
                                                A[
                                                   Text numberString;
                                                   Attr.Class "bold marR"; 
                                                   Attr.Target "_blank"; 
                                                   (Attr.HRef <|  (if unprovisioned then "" else ("/portal/Addresses#"+(x.Number.Substring(1)))));                                                   
                                                ];
                                                (if unprovisioned then Span[Text " [unprovisioned] "; Attr.Style "color:#9d261d;"; Attr.Class "size-2"] else Span[Text ""]);
                                            ]
                                            TD[Text <| (screenUndefined  x.SrcIP) + "||" + x.AccountId.ToString()]
                                            TD[Attr.Class "bold"; Text <| (screenUndefined  x.SipCode)]
                                            TD[Text <| (screenUndefined  x.Service)]
                                            TD[Text <| (formatDate2 x.AnsweredOn)]
                                            TD[Text <| x.Duration + " secs" ]
                                        ]
                            table.Append(row)
                            table.Append(TR[Attr.Class <| "hide details"; Attr.Style "";] 
                                -< [
                                    TD[Attr.ColSpan "7"; Attr.Style "background-color:#F9F9F9";] -< [
                                        B[Text "Call ID: "];
                                        Span[Text (screenUndefined  x.CallID)];
                                        (if unprovisioned then Span[Text " [unprovisioned] "; Attr.Style "color:#9d261d;"] else Span[Text ""]);
                                    ]
                                ])
                            JE(row).Click(fun e k -> JE(row).ToggleClass("detailsParent").Next().Toggle().Ignore) |> ignore
                        )
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
        catchEnterKey (JE(SrcIP)) (JE(btn))
        catchEnterKey (JE(DstIP)) (JE(btn))
        catchEnterKey (JE(SipCode)) (JE(btn))
        catchEnterKey (JE(Start)) (JE(btn))
        catchEnterKey (JE(End)) (JE(btn))
        JE(dlBtn).Click(fun e k ->                     
                    Async.Start(
                        async {
                            let! dlRes = Api.EmailAllCdrsAsAttachment<| bindCrit()                                           
                            if dlRes.Success then
                                InfoAlert "Due to the file size, the CDRs will be sent to you by email within the next few minutes."
                            else
                                Error dlRes.ErrorInfo msgHolder
                        }
                    )
                ).Ignore        
                            
        //compose
        authScreen <|
            parent 
                -< [
                    H3[Attr.Class "blue"; Text "Search CDRs"; Attr.Style "margin-top:-6px;";]
                    Tel;
                    SrcIP;
                    DstIP;
                    SipCode;
                    Start;
                    End;
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