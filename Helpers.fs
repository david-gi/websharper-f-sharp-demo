module Helpers

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Web
open IntelliFactory.WebSharper.JQuery

///JS Alert
[<JavaScript>]
let Al = JavaScript.Alert

[<JavaScript>]
let JE (el: Element) = JQuery.Of(el.Dom)

[<JavaScript>]
let J (el: Dom.Element) = JQuery.Of(el)

[<JavaScript>]
let JD (el: Dom.Node) = JQuery.Of(el)

[<JavaScript>]
let JS (el: string) = JQuery.Of(el)

///clear line with Hr
[<JavaScript>]
let Cll() = Hr[Attr.Class "clear"]

///clear line
[<JavaScript>]
let Cl() = Br[Attr.Class "clear"]

[<JavaScript>]
let listObj (xs: (string * obj) list) =
    let x = obj()
    xs |> List.iter(fun (n,v) -> JavaScript.Set x n v)
    x
    
[<JavaScript>]
let screenUndefined x = (match x with | "undefined" -> "" | v -> v)


[<JavaScript>]
let addressCodeString (code: int) = 
    match code with
    | 0 -> "Valid"
    | 1 -> "Corrected"
    | 2 -> "Invalid"
    | 3 -> "Internal Error"
    | _ -> ""

[<JavaScript>]
let maskNumber (numStr: string)  = 
        //"(999) 999-9999"
        let mutable nums = numStr.Replace("(","").Replace(")","").Replace(" ","").Replace("-","")
        if nums.StartsWith("1") then nums <- nums.Substring(1,nums.Length - 1)
        try
            nums <- "(" + nums.Substring(0,3) + ") " + nums.Substring(3,3) + "-" + nums.Substring(6,4)
            nums
        with _ -> numStr

[<JavaScript>]
let maskNumberInput (jnumTB: JQuery)  =            
        let v = jnumTB.Val() :?> string
        let nums = maskNumber v
        if nums.Length > 0 then jnumTB.Val(nums) |> ignore
        
[<JavaScript>]
let screenKeys (which: int)  =  
    let keys = [|8;46;17;16; 35;36;37;38;39;40;65;|]
    not(keys |> Array.exists(fun x -> x = which))

[<JavaScript>]
let cleanNumber (numStr: string) = 
        numStr.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")

[<JavaScript>]
let refresh () = (JavaScript.SetTimeout Html5.Window.Self.Location.Reload  1000) |> ignore

[<JavaScript>]
let redirect (action : Api.Action) = 
    Html5.Window.Self.Location.Replace <|
        match action with
        | Api.Action.Addresses -> "/portal/Addresses"
        | Api.Action.ApiKeys -> "/portal/ApiKeys"
        | Api.Action.Users -> "/portal/Users"
        | Api.Action.Gateways -> "/portal/Gateways"
        | Api.Action.Alerts -> "/portal/Alerts"
        | Api.Action.Login -> "/portal/Login"
        | Api.Action.Dashboard -> "/portal/"
        | Api.Action.AdminDashboard -> "/portal/Admin"
        | Api.Action.AdminLogin -> "/portal/Admin/Login"
        | Api.Action.AdminLogout -> "/portal/Admin/Logout"
        | _  -> "/portal/"

[<JavaScript>]
let IsNullEmptyOrUndefined value = value = null || value = "" || value = "undefined"  || value = "null" || value = JavaScript.Undefined

    
[<JavaScript>]
let buildAddrStr (addr: Api.Address array) =
    if addr.Length = 0 then "" else
    let addr = addr.[0]
    screenUndefined addr.HouseNumber + " " +
    screenUndefined addr.HouseNumberSuffix + " " +
    (if IsNullEmptyOrUndefined addr.RoadPreDir then "" else addr.RoadPreDir + " ") +
    screenUndefined addr.Road + ", " +
    (if IsNullEmptyOrUndefined addr.RoadSuffix then "" else addr.RoadSuffix + " ") +
    (if IsNullEmptyOrUndefined addr.RoadPostDir then "" else addr.RoadPostDir + " ") +
    (if IsNullEmptyOrUndefined addr.Location then "" else addr.Location + ", ") +
    screenUndefined addr.Community + ", " +
    (screenUndefined addr.State).ToUpper() + ", " +
    screenUndefined addr.Country + ", " +
    screenUndefined addr.PostalCode

[<JavaScript>]
let setPingTimer() =
    JavaScript.SetInterval 
        (fun () -> Async.Start( async{
            let! res = Api.Ping()
            if not res then Api.ShowLoginOverlay()
        } ) )  
        180000 
        |> ignore

[<JavaScript>]
let authScreen (contentFun : IPagelet) : IPagelet =
    let rEl = Div [Text "Redirecting..."; Attr.Class "gray bold"] :> IPagelet
    let showAdminMenu () = JS("#AdminMenu").Show() |> ignore 
    let noAccountId = IsNullEmptyOrUndefined Api.Vars.AccountId
    let noApiKey = IsNullEmptyOrUndefined Api.Vars.ApiKey
    let isAdmin = Api.Vars.IsAdmin;
    let hasAdmin = Html5.Window.Self.Location.Pathname.Contains("/Admin")
    let accountIdNotZero = not(Api.Vars.AccountId = "0")
    
    //JavaScript.Alert ("noAccountId:"+(noAccountId.ToString())  + " | " + "noApiKey:"+(noApiKey.ToString()) + " | "  + "isAdmin:"+(isAdmin.ToString())  + " | "  + "hasAdmin:"+(hasAdmin.ToString())  + " | "  + "accountIdNotZero:"+(accountIdNotZero.ToString()) )

    if isAdmin then
        if noApiKey then redirect Api.Action.AdminLogin; rEl
        else if not(hasAdmin) && not(accountIdNotZero) then  redirect Api.Action.AdminDashboard; rEl
        else
            JS("#AdminAsAccountName").Val("Logged in as: " + Api.Vars.AccountName).Show() |> ignore
            showAdminMenu()
            setPingTimer()
            contentFun
    else
        if noAccountId || noApiKey then 
            if hasAdmin then redirect Api.Action.AdminLogin; rEl
            else redirect Api.Action.Login; rEl
        else 
            setPingTimer()
            contentFun
     
///In element message alert with (content: Element)
[<JavaScript>]
let MessageAlert2 (content : Element) (parent : Element)  (alertClass : string) = 
    let msgBox = Div[Attr.Class ("alert marT2 MessageAlert " + alertClass.ToString());]
    let closeBtn = Span[Text "x"; Attr.Class "bold right pointer"; Attr.Style "font-family:Lucida Console, Monaco5, monospace;margin-right:-12px;";]
    JE(closeBtn).Click(fun _ _ -> JE(msgBox).Remove() |> ignore) |> ignore
    JE(parent).Append (msgBox -< [closeBtn; content; Cl()]).Dom |> ignore

///In element message alert
[<JavaScript>]
let MessageAlert (msg : string) (parent : Element)  (alertClass : string) = 
    let msgBox = Div[Attr.Style "max-width:550px; margin-bottom:-12px;"; 
             Attr.Class ("alert marT2 MessageAlert " + alertClass.ToString()); Text msg]
    let closeBtn = Span[Text "x"; Attr.Class "bold right pointer"; Attr.Style "font-family:Lucida Console, Monaco5, monospace;margin-right:-12px;";]
    JE(closeBtn).Click(fun _ _ -> JE(msgBox).Remove() |> ignore) |> ignore
    JE(parent).Append (msgBox -< [closeBtn;]).Dom |> ignore
    
[<JavaScript>]
let ClearMessageAlerts () = JS(".MessageAlert").Hide().Remove() |> ignore
    
[<JavaScript>]
let Success (msg : string) (parent : Element) = MessageAlert msg parent "alert-success"
                                                   
[<JavaScript>]                                     
let Warning (msg : string) (parent : Element) = MessageAlert msg parent "alert-warning"
                                                  
[<JavaScript>]                                    
let Info (msg : string) (parent : Element)  = MessageAlert msg parent "alert-info"
                                                    
[<JavaScript>]                                      
let Error (msg : string) (parent : Element) = MessageAlert msg parent "alert-error"

///Popup error alert with a message
[<JavaScript>]
let ErrorAlert x = 
    JQuery.JQuery.Of("body").Prepend(
                    "   <div id='errorAlert' class='dropdown-menu alert alert-error pad2' style='background-color:#F2DEDE; width:300px; position:fixed; top:100px; left:30%; display:block;'>"+
                    "   <span class='icon-warning-sign'><b style='margin-left:16px'>Error:</span></b><br />" +
                    x +
                    "   <br /><br /><span class='gray size-2 centerTxt' style='width:100%'>(double-click to close)</span>" +
                    "   </div>"+
                    "<script type='text/javascript'>$('#errorAlert').dblclick(function(){ $('#errorAlert').remove(); })</script>"
                    ) |> ignore
                    
///Popup message alert with a message
[<JavaScript>]
let InfoAlert x = 
    JQuery.JQuery.Of("body").Prepend(
                    "   <div id='infoAlert' class='dropdown-menu alert alert-info pad2' style='background-color:#DEECF2; width:300px; position:fixed; top:100px; left:30%; display:block;'>"+
                    x +
                    "   <br /><br /><span class='gray size-2 centerTxt' style='width:100%'>(double-click to close)</span>" +
                    "   </div>"+
                    "<script type='text/javascript'>$('#infoAlert').dblclick(function(){ $('#infoAlert').remove(); })</script>"
                    ) |> ignore
                    
//HTML Input

///text & callback
[<JavaScript>]
let ConfirmDeleteButton txt (clickFun: unit -> unit) =
    let btn = Button[Text txt; Attr.Class "btn btn-danger"; Attr.Style "min-width:80px;"]
    let isSnd = ref false;
    let jbtn = JE(btn)
    jbtn.Click(fun e o ->
            if !isSnd then
                clickFun()
                jbtn.RemoveClass("btn-warning").AddClass("btn-danger").Text(txt) |> ignore
                isSnd := false
            else
                isSnd := true
                jbtn.RemoveClass("btn-danger").AddClass("btn-warning").Text("Confirm") |> ignore
                JavaScript.SetTimeout (fun()-> isSnd := false; jbtn.RemoveClass("btn-warning").AddClass("btn-danger").Text(txt) |> ignore; ()) 2000 |> ignore
        ) |> ignore
    btn
    
///text & callback with dom element
[<JavaScript>]
let ConfirmDeleteButton2 txt (clickFun: Dom.Element -> unit) =
    let btn = Button[Text txt; Attr.Class "btn btn-danger"; Attr.Style "min-width:80px;"]
    let isSnd = ref false;
    let jbtn = JE(btn)
    jbtn.Click(fun e o ->
            if !isSnd then
                clickFun e
                jbtn.RemoveClass("btn-warning").AddClass("btn-danger").Text(txt) |> ignore
                isSnd := false
            else
                isSnd := true
                jbtn.RemoveClass("btn-danger").AddClass("btn-warning").Text("Confirm") |> ignore
                JavaScript.SetTimeout (fun()-> isSnd := false; jbtn.RemoveClass("btn-warning").AddClass("btn-danger").Text(txt) |> ignore; ()) 2000 |> ignore
        ) |> ignore
    btn

[<JavaScript>]
let EditSaveButton txt (firstClickFun: Dom.Element -> unit) (secondClickFun: Dom.Element -> unit) =
    let btn = Button[Text txt; Attr.Class "btn btn-info"; Attr.Style "min-width:72px;"]
    let isSnd = ref false;
    let jbtn = JE(btn)
    jbtn.Click(fun e o ->
            if !isSnd then
                secondClickFun e
                jbtn.RemoveClass("btn-primary").AddClass("btn-info").Text(txt) |> ignore
                isSnd := false
            else
                firstClickFun e
                isSnd := true
                jbtn.RemoveClass("btn-info").AddClass("btn-primary").Text("Save") |> ignore
        ) |> ignore
    btn

///get nested input value
[<JavaScript>]
let GetInputVal (el : Element) = (JD(el.Dom.LastChild).Val() :?> string).Trim()
///get nested input value and set to element value
[<JavaScript>]
let GetSetInputVal (el : Element) = el.Value <- JD(el.Dom.LastChild).Val() :?> string

///set nested input value
[<JavaScript>]
let SetInputVal (el : Element) x = JD(el.Dom.LastChild).Val(screenUndefined x)

[<JavaScript>]
let LabelledInput (label: string) (ttype: string) (cclass : string) (value: string) =
    Div[Attr.Class "left marR"] -< [
        Abbr[Text label; Attr.Class "attribute bold"; Attr.Style "font-size:1.2em; color:#111;" ] -< [
            I[ Attr.Class "icon-info-sign faded"; Attr.Style "margin-left:2px;"]
        ]
        Br[]
        Input[Attr.Type ttype; Attr.Value value; Attr.Class cclass]
    ]
    
///label, class, value
[<JavaScript>]
let Textbox (label: string) (cclass : string) (value: string) = LabelledInput label "" "text" value

///label, tip, input type, class, value
[<JavaScript>]
let TippedInput (label: string) (tip: string) (ttype: string) (cclass : string) (value: string) =
    Div[Attr.Class "left marR"] -< [
        Abbr[Text label; Attr.Title tip; Attr.Class "attribute bold"; Attr.Style "font-size:1.1em; color:#111;" ] -< [
            I[ Attr.Class "icon-info-sign faded"; Attr.Style "margin-left:2px;"]
        ]
        Br[]
        Input[Attr.Type ttype; Attr.Value value; Attr.Class (cclass + " inputmask");]
    ]
    
///label, tip, class, value
[<JavaScript>]
let TextboxTip (label: string) (tip : string) (cclass : string) (value: string) = TippedInput label tip "text" cclass value

///label, tip, example text, input type, class, value
[<JavaScript>]
let TextboxTip2 (label: string) (tip: string) (exampleText: string) (cclass : string) (value: string) =
    Div[Attr.Class "left marR"] -< [
        Abbr[Text label; Attr.Title (tip + " " + exampleText); Attr.Class "attribute bold"; Attr.Style "font-size:1.1em; color:#111;" ] -< [
            I[ Attr.Class "icon-info-sign faded"; Attr.Style "margin-left:2px;"]
        ]
        Br[]
        Input[Attr.Type "text"; Attr.Value value; Attr.Class (cclass + " inputmask"); NewAttr "placeholder" exampleText]
    ]


///label, tip, value, class, value*text list
[<JavaScript>]
let Dropdown (label: string) (tip: string)  (value : string) (cclass : string) (options : (string*string) List) = 
    let select = Default.Select[Attr.Class cclass]
    let jselect = JE(select)
    options |> List.iter (fun (o : string*string) -> jselect.Append("<option value='"+fst o+"'>"+snd o+"</option>") |> ignore)
    jselect.Val(value) |> ignore
    
    Div[Attr.Class "left marR"] -< [
        Abbr[Text label; Attr.Title tip; Attr.Class "attribute bold "; Attr.Style "font-size:1.1em; color:#111;"; ] -< [
            I[ Attr.Class "icon-info-sign faded"; Attr.Style "margin-left:2px;"]
        ]
        Br[]
        select
    ]
    
[<JavaScript>]
let DirectionDrop (label: string) (tip: string) (value: string) =
    Dropdown label tip value "input-mini" ["","";"N","N";"W","W";"E","E";"S","S";"NW","NW";"NE","NE";"SW","SW";"SE","SE";]

[<JavaScript>]
let InputHelp msg = Span[Text msg; Attr.Class "help-inline"]

[<JavaScript>]
let formatDate d = screenUndefined(d.ToString()).Substring(0, 10)

[<JavaScript>]
let formatDate2 d = 
    try
        let splt = screenUndefined(d.ToString()).Substring(0, 16).Split([|'T'|]);
        let splt2 = (splt.[1]).Split([|':'|]);
        let mutable hr = System.Int32.Parse(splt2.[0])
        let tt = if hr > 12 then hr <- hr - 12; "PM" else "AM"        
        splt.[0] + " " + hr.ToString() + ":" + splt2.[1] + " " + tt
    with _ -> ""

[<JavaScript>]
let catchEnterKey (jEl: JQuery.JQuery) (jBtn: JQuery.JQuery) = jEl.Keypress(fun _ k -> if k.Which = 13 then jBtn.Click() |> ignore) |> ignore

//Validation
[<JavaScript>]
let Required parent (fields: Element list) = 
    let valid = ref true
    fields |> List.iter(fun el -> if GetInputVal(el).Trim() = "" then 
                                     if not !valid then Error (JD(el.Dom.FirstChild).Text() + " required.") parent
                                     valid := false; 
                                     JE(el).AddClass("control-group error input-error").Ignore
                                  else JE(el).RemoveClass("control-group error input-error").Ignore)
    valid

[<JavaScript>]
let ValidIp parent (el: Element) = 
    //basic validate ip - websharper doesn't translate IpAddress.Parse or regex
    let ipString = GetInputVal(el).Trim()
    try
        if ipString = "" then false
        else
            //ipv6
            let ipSplt1 = ipString.Split(':')
            if ipSplt1.Length > 8 then failwith ""
            if ipSplt1.Length = 8 then ipSplt1 |> Array.iter(fun x -> if x.Length > 4 then failwith "")

            else
                //ipv4
                let ipSplt2 = ipSplt1.[0].Split('.') 
                if not(ipSplt2.Length = 4) then failwith ""
                ipSplt2 |> Array.iter(fun x -> 
                                        let n = System.Int32.Parse(x) 
                                        if not(n > 0 && n < 256) then failwith "")
        
            JE(el).RemoveClass("control-group error input-error").Ignore
            true
    with _ ->
        Error (JD(el.Dom.FirstChild).Text() + " is not valid.") parent
        JE(el).AddClass("control-group error input-error").Ignore
        false