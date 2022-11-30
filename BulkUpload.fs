module Demo911App.BulkUpload

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery
open System.IO;
open Helpers
  
type BulkUploadControl() = 
    inherit Control()
    
    [<JavaScript>]
    override this.Body =  
        let content = 
            //create
            let parent = Div[ Attr.Class "control-group success"]
            let sbox = TextArea[Id "sbox"; Attr.Style "min-width:620px;";]
            let btn = Button[ 
                                Id "btn"; Text "Upload"; Attr.Class "btn btn-success"; 
                                Attr.Style "height:40px; margin-left:4px; margin-bottom:2px;";
                                NewAttr "data-loading-text" "Saving..." 
                            ]
                      
            //js
            let jBtn = JE(btn)
            jBtn.Click(fun _ _ -> 
                    try
                                let lines = (string <| JE(sbox).Val()).Split('\n')
                                lines |> Array.iter(fun x ->
                                                    let cols = x.Split(',')
                                                    let sub = { Api.Subscriber.Empty with 
                                                                    Number = cols.[0];
                                                                    Address = { Api.Address.Empty with
                                                                                    SubscriberName = cols.[1];
                                                                                    HouseNumber = cols.[2];
                                                                                    Road = cols.[3];
                                                                                    Location = cols.[4];
                                                                                    Community = cols.[5];
                                                                                    State = cols.[6];
                                                                                    Country = "US";
                                                                                    PostalCode = cols.[7];
                                                                                }
                                                              }
                                                    Async.Start(
                                                        async {
                                                                let! subs = Api.SetSubscriber(sub) 
                                                                match subs with
                                                                | x when x.Success -> Success("Address added: " + sub.Number) parent |> ignore
                                                                | x  -> Warning("Address error: " + sub.Number) parent |> ignore
                                                      }
                                                  )
                                                )
                      with x ->  Warning("Address error") parent |> ignore
                      ) |> ignore
            //compose            
            parent
                -< [
                    Span [Attr.Class "input-prepend"] -<
                    [
                        sbox
                    ]
                    btn
                ]
        
        authScreen <| 
                Div [
                    H3[Html.Default.Text "Bulk Address Upload"; Html.Default.Attr.Style "margin-top:-6px;"; Html.Default.Attr.Class "blue";] 
                    B[Text "Add or Update Addresses in bulk (100 max at a time)"] 
                    Br[]
                    I[Text "(One per-line: Number, SubscriberName, HouseNumber, Road, Location, Community, State, Country, PostalCode)";]; 
                    content
                ]
