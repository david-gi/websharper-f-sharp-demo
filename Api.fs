module Api

open System
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.JQuery

[<JavaScript>]
let X<'a> = Unchecked.defaultof<'a>

type Action =
    | Login
    | Logout
    | Dashboard
    | Addresses
    | BulkUpload
    | ApiKeys
    | Users
    | Gateways
    | Alerts
    | Cdrs
    | AdminLogin
    | AdminLogout
    | AdminDashboard

type Result<'a> = {
    Success : bool
    ErrorCode : int
    ErrorInfo : string
    Data : 'a }

type Empty = obj

[<Flags>]
type Roles = 
    | Read = 0
    | Owner = 1
    
type Account = {
    AccountId : Int32
    CrmAccountId : String
    Enabled : Boolean
    Email : String
    ContactName : String
    ContactCompany : String
    ContactAddress : String
    ContactCity : String
    ContactState : String
    ContactPostal : String
    ContactCountry : String
    SignUpIP : String
    CreatedOn : DateTime
} with
    [<JavaScript>]
    static member Empty = { AccountId = X<_>; CrmAccountId = ""; Enabled = X<_>; Email = ""; ContactName = ""; ContactCompany = ""; ContactAddress = ""; ContactCity = ""; ContactState = ""; ContactPostal = ""; ContactCountry = ""; SignUpIP = ""; CreatedOn = X<_>; }
    
type Address = {
    AddressId : Int64
    AccountId : Int32
    Number : String
    _InternalStatus : Int32
    CreatedOn : DateTime
    DeletedOn : DateTime
    SubscriberName : String
    HouseNumber : String
    HouseNumberSuffix : String
    RoadPreDir : String
    Road : String
    RoadSuffix : String
    RoadPostDir : String
    Community : String
    State : String
    PostalCode : String
    Country : String
    Location : String
} with
    [<JavaScript>]
    static member Empty = { AddressId = X<_>; AccountId = X<_>; Number = ""; _InternalStatus = X<_>; CreatedOn = X<_>; DeletedOn = X<_>; SubscriberName = ""; HouseNumber = ""; HouseNumberSuffix = ""; RoadPreDir = ""; Road = ""; RoadSuffix = ""; RoadPostDir = ""; Community = ""; State = ""; PostalCode = ""; Country = ""; Location = ""; }
    
type AddressStatus = {
    AddressCode : Int32
    Message : String
    Alternatives : Address array
} with
    [<JavaScript>]
    static member Empty = { AddressCode = X<_>; Message = ""; Alternatives = X<_>; }
    
type AdminUser = {
    AdminUserId : Int32
    Email : String
    Name : String
    Roles : Roles
} with
    [<JavaScript>]
    static member Empty = { AdminUserId = X<_>; Email = ""; Name = ""; Roles = X<_>; }
    
type AlertTarget = {
    AlertTargetId : Int32
    AlertListId : Int32
    Name : String
    Email : String
    Sms : String
    Phone : String
    HttpCallback : String
    AttachRecording : Boolean
    AllowStreaming : Boolean
} with
    [<JavaScript>]
    static member Empty = { AlertTargetId = X<_>; AlertListId = X<_>; Name = ""; Email = ""; Sms = ""; Phone = ""; HttpCallback = ""; AttachRecording = X<_>; AllowStreaming = X<_>; }
    
type AlertList = {
    AlertListId : Int32
    AccountId : Int32
    Name : String
    GroupPath : String
    DeleteWhenUnused : Boolean
    Targets : AlertTarget[]
} with
    [<JavaScript>]
    static member Empty = { AlertListId = X<_>; AccountId = X<_>; Name = ""; GroupPath = ""; DeleteWhenUnused = X<_>; Targets = X<_>; }
    
type ApiKey = {
    ApiKeyId : Int32
    AccountId : Int32
    IsAdmin : Boolean
    UserId : Int32
    Roles : Roles
    CreatedOn : DateTime
    ExpiresOn : DateTime
    IPAddress : String
    IPMask : Int32
    GroupPath : String
} with
    [<JavaScript>]
    static member Empty = { ApiKeyId = X<_>; AccountId = X<_>; IsAdmin = X<_>; UserId = X<_>; Roles = X<_>; CreatedOn = X<_>; ExpiresOn = X<_>; IPAddress = ""; IPMask = X<_>; GroupPath = ""; }
    
type AuditLog = {
    AuditLogId : Int64
    AccountId : Int32
    UserId : Int32
    IsAdmin : Boolean
    Number : String
    Action : Int32
    ActionData : String
    IP : String
    ClientIP : String
    CreatedOn : DateTime
} with
    [<JavaScript>]
    static member Empty = { AuditLogId = X<_>; AccountId = X<_>; UserId = X<_>; IsAdmin = X<_>; Number = ""; Action = X<_>; ActionData = ""; IP = ""; ClientIP = ""; CreatedOn = X<_>; }
    
type BrandedSite = {
    BrandedSiteId : Int32
    AccountId : Int32
    SiteKey : String
    HostName : String
    CSS : Byte[]
    GroupPath : String
} with
    [<JavaScript>]
    static member Empty = { BrandedSiteId = X<_>; AccountId = X<_>; SiteKey = ""; HostName = ""; CSS = X<_>; GroupPath = ""; }
    
type Gateway = {
    GatewayId : Int32
    AccountId : Int32
    CreatedOn : DateTime
    DeletedOn : DateTime
    IP : String
} with
    [<JavaScript>]
    static member Empty = { GatewayId = X<_>; AccountId = X<_>; CreatedOn = X<_>; DeletedOn = X<_>; IP = ""; }

type Subscriber = {
    Number : String
    AccountId : Int32
    Address : Address
    AlertListId : Int32
    GroupPath : String
    CreatedOn : DateTime
    LastUpdated : DateTime
    DeletedOn : DateTime
    NextTestSendsAlerts : Boolean
    AddressStatus : AddressStatus
} with
    [<JavaScript>]
    static member Empty = { Number = ""; AccountId = -1; Address = Address.Empty; AlertListId = X<_>; GroupPath = ""; CreatedOn = X<_>; LastUpdated = X<_>; DeletedOn = X<_>; AddressStatus = AddressStatus.Empty; NextTestSendsAlerts = X<_>; }
    
type User = {
    UserId : Int32
    Email : String
    Name : String
    AccountId : Int32
    Roles : Roles
    GroupPath : String
    IPAddress : String
    IPMask : Int32
} with
    [<JavaScript>]
    static member Empty = { UserId = X<_>; Email = ""; Name = ""; AccountId = X<_>; Roles = X<_>; GroupPath = ""; IPAddress = ""; IPMask = X<_>; }
 
type Cdr = {
    CdrId: Int64
    AccountId : Int32 option
    Service : String
    Number : String
    AddressId : Int32 option
    Machine : String
    SrcIP : String
    DstIP : String
    CallID : String
    Uuid : Guid
    SipCode : String
    CreatedOn : DateTime
    ProgressOn : DateTime option
    AnsweredOn : DateTime option
    Duration : String
    HangupDirect : Int32
    CdrData : String
    QueuedOn : DateTime
    ArchivedOn : DateTime
} 

type CdrSearchCriteria = {
    Number : string
    Service : string
    Machine : string
    SrcIP : string
    DstIP : string
    CallID : string
    SipCode : string
    HangupDirection : int
    CreatedOnStart : string
    CreatedOnEnd :  string
    Size : int
    Index : int
} with
    [<JavaScript>]
    static member Empty = { Number = ""; Service = ""; Machine = ""; SrcIP = ""; DstIP = ""; CallID = ""; SipCode = ""; HangupDirection = X<_>; CreatedOnStart = ""; CreatedOnEnd =  ""; Size = 50; Index = 0; }
 

type Vars() =
    [<JavaScript>]
    static member IsAdmin = try (IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("isadmin")) = "true" with _ -> false
    [<JavaScript>]
    static member ApiKey = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("apikey")
    [<JavaScript>]
    static member AccountId = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("accountid")
    [<JavaScript>]
    static member AdminAccountId = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("adminaccountid")
    [<JavaScript>]
    static member AlertNumber = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("alertnumber")
    [<JavaScript>]
    static member Number = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("number")
    [<JavaScript>]
    static member AlertName = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("name")
    [<JavaScript>]
    static member AccountName = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.GetItem("accountname")
    [<JavaScript>]
    static member SetIsAdmin v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("isadmin", v) 
    [<JavaScript>]
    static member SetApiKey v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("apikey", v) 
    [<JavaScript>]
    static member SetAccountId v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("accountid", v) 
    [<JavaScript>]
    static member SetAdminAccountId v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("adminaccountid", v) 
    [<JavaScript>]
    static member SetAccountName v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("accountname", v) 
    [<JavaScript>]
    static member SetAlertName v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("name", v) 
    [<JavaScript>]
    static member SetAlertNumber v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("alertnumber", v) 
    [<JavaScript>]
    static member SetNumber v = IntelliFactory.WebSharper.Html5.Window.Self.LocalStorage.SetItem("number", v) 

[<JavaScript>]
let listObj (xs: (string * obj) list) =
    let x = obj()
    xs |> List.iter(fun (n,v) -> JavaScript.Set x n v)
    x

[<JavaScript>]
let ShowLoginOverlay() = 
    JQuery.Of("#LoginOverlay").FadeIn().Ignore
    JQuery.Of("#LoginOverlay :input:first-child").Focus().Ignore
    
[<JavaScript>]
let PopError msg =
    JQuery.JQuery.Of("body").Prepend(
                "   <div id='errorAlert' class='dropdown-menu alert alert-error' style='width:300px; position:fixed; padding:24px; background-color:#F2DEDE; top:100px; left:30%; display:block;'>"+
                "   <span class='icon-warning-sign'><b style='margin-left:16px'>Error:</span></b><br />" +
                msg +
                "   <br /><br /><span class='gray size-2 centerTxt' style='width:100%'>(double-click to close)</span>" +
                "   </div>"+
                "<script type='text/javascript'>$('#errorAlert').dblclick(function(){ $('#errorAlert').remove(); })</script>"
                ) |> ignore

[<JavaScript>]
let asyncPost url (data: 'd) (creds: (string * string) option) : Async<'a> =
    Async.FromContinuations (fun (ok, err, _) ->
        try            
            let conf =
                JQuery.AjaxConfig(
                    Async = true,
                    Url = url,
                    Data = data,
                    Type = ((box RequestType.POST) :?> RequestType),
                    Success = [| fun (res, _, _) -> ok (As<'a> res) |],
                    Error = [| fun (jqXHR, msg, _) -> 
                            let statusCode = (box jqXHR :?> JqXHR).StatusText
                            if statusCode.Contains("403") || statusCode.Contains("Forbidden") then 
                                Vars.SetAccountId JavaScript.Undefined
                                Vars.SetAccountName JavaScript.Undefined
                                if not Vars.IsAdmin then
                                    Vars.SetIsAdmin JavaScript.Undefined
                                    Vars.SetApiKey JavaScript.Undefined
                                else
                                    Vars.SetAccountId "0"  
                                match Vars.IsAdmin with 
                                | true -> 
                                        Html5.Window.Self.Location.Replace "/portal/Admin"
                                | _ -> ShowLoginOverlay()
                            err (failwith msg);|]
                )
            match creds with
            | None             -> ()
            | Some (user,pass) -> 
                conf.BeforeSend <- (fun xhr -> let xhr = xhr :?> JQuery.JqXHR
                                               xhr.SetRequestHeader("Authorization", "Basic " + user + ":" + pass)
                                               )
            JQuery.Ajax(conf) |> ignore
        
        with x -> PopError x.Message
            
    )

[<JavaScript>]
let asyncApiPost (url: string) (data: 'd) =
    async {
        try
            let creds = if Vars.ApiKey ==. JavaScript.Undefined then None else Some (Vars.AccountId, Vars.ApiKey)
            let! x = asyncPost url data creds
            if (not x.Success) && x.ErrorCode = 6 then
                match Vars.IsAdmin with 
                | true -> Html5.Window.Self.Location.Replace "/portal/Admin/Login"
                | _ -> ShowLoginOverlay()
            return x
        with ex -> 
            let errRes = {
                    Success=false;
                    ErrorCode=4;
                    ErrorInfo=ex.Message;
                    Data= Object() :?> 'a; 
                }
            return errRes
    }
    
[<JavaScript>]
let asyncPostNoRedirect url (data: 'd) (creds: (string * string) option) : Async<'a> =
    Async.FromContinuations (fun (ok, err, _) ->
        try
            let conf =
                JQuery.AjaxConfig(
                    Async = true,
                    Url = url,
                    Data = data,
                    Type = ((box RequestType.POST) :?> RequestType),
                    Success = [| fun (res, _, _) -> ok (As<'a> res) |],
                    Error = [| fun (jqXHR, msg, _) -> 
                            err (failwith msg);|]
                )
            match creds with
            | None             -> ()
            | Some (user,pass) -> 
                conf.BeforeSend <- (fun xhr -> let xhr = xhr :?> JQuery.JqXHR
                                               xhr.SetRequestHeader("Authorization", "Basic " + user + ":" + pass)
                                               )
            JQuery.Ajax(conf) |> ignore
        
        with x -> PopError x.Message
            
    )

[<JavaScript>]
let asyncApiPostNoRedirect (url: string) (data: 'd) =
    async {
        let creds = if Vars.ApiKey ==. JavaScript.Undefined then None else Some (Vars.AccountId, Vars.ApiKey)
        let! x = asyncPost url data creds
        return x
    }
    
[<JavaScript>]
let Ping () : Async<Boolean>  = 
    async {
        try
            let url = "/api/Accounts/Ping"
            let data = listObj [ ]
            let! res = asyncApiPost url data
            let r = box res :?> Result<String> 
            return r.Success
        with _ -> return false
    }

// AccountsController
[<JavaScript>]
let Login (email: String) (password: String) : Async<Result<String>>  = async {
    let url = "/api/Accounts/Login"
    let data = listObj [ "email", box email; "password", box password;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<String> }
    
[<JavaScript>]
let Logout  = async {      
    Vars.SetAccountId JavaScript.Undefined
    Vars.SetAccountName JavaScript.Undefined
    if not Vars.IsAdmin then
        Vars.SetIsAdmin JavaScript.Undefined
        Vars.SetApiKey JavaScript.Undefined
    else
        Vars.SetAccountId "0"  
    let url = "/api/Accounts/Logout"
    let data = listObj [ ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let ChangePassword (oldPassword: String) (newPassword: String)  = async {
    let url = "/api/Accounts/ChangePassword"
    let data = listObj [ "oldPassword", box oldPassword; "newPassword", box newPassword;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let ResetPassword (userId: Int32) (newPassword: String)  = async {
    let url = "/api/Accounts/ResetPassword"
    let data = listObj [ "userId", box userId; "newPassword", box newPassword;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }

[<JavaScript>]
let GetAccount (accountId: Int32)  = async {
    let url = "/api/Accounts/GetAccount"
    let data = listObj [ "accountId", box accountId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Account> }
    
[<JavaScript>]
let EditAccount (account: Account)  = async {
    let url = "/api/Accounts/EditAccount"
    let data = listObj [ "account", box account;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Account> }

[<JavaScript>]
let CreateUser (user: User) (password: String)  = async {
    let url = "/api/Accounts/CreateUser"
    let data = listObj [ "user", box user; "password", box password;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Int32> }
    
[<JavaScript>]
let EditUser (user: User)  = async {
    let url = "/api/Accounts/EditUser"
    let data = listObj [ "user", box user;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let ListUsers (accountId: Int32)  = async {
    let url = "/api/Accounts/ListUsers"
    let data = listObj [ "accountId", box accountId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<User[]> }
    
[<JavaScript>]
let DeleteUser (userId: Int32)  = async {
    let url = "/api/Accounts/DeleteUser"
    let data = listObj [ "userId", box userId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let ListKeys (accountId: Int32)  = async {
    let url = "/api/Accounts/ListKeys"
    let data = listObj [ "accountId", box accountId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<ApiKey[]> }
    
[<JavaScript>]
let CreateKey (key: ApiKey)  = async {
    let url = "/api/Accounts/CreateKey"
    let data = listObj [ "key", box key;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<String> }
    
[<JavaScript>]
let DeleteKey (keyId: Int32)  = async {
    let url = "/api/Accounts/DeleteKey"
    let data = listObj [ "keyId", box keyId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let AddGateway (ip: String) (accountId: Int32)  = async {
    let url = "/api/Accounts/AddGateway"
    let data = listObj [ "ip", box ip; "accountId", box accountId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let ListGateways (accountId: Int32)  = async {
    let url = "/api/Accounts/ListGateways"
    let data = listObj [ "accountId", box accountId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Gateway[]> }
    
[<JavaScript>]
let DeleteGateway (gatewayId: Int32)  = async {
    let url = "/api/Accounts/DeleteGateway"
    let data = listObj [ "gatewayId", box gatewayId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }

// AdminAccountsController
module Admin =
    [<JavaScript>]
    let Login (email: String) (password: String)  = async {
        let url = "/api/AdminAccounts/Login"
        let data = listObj [ "email", box email; "password", box password;  ]
        let! res = asyncApiPost url data
        return box res :?> Result<String> }
            
    [<JavaScript>]
    let Logout  = async {
        Vars.SetAccountId JavaScript.Undefined
        Vars.SetAccountName JavaScript.Undefined
        Vars.SetApiKey JavaScript.Undefined
        Vars.SetIsAdmin "false"
        let url = "/api/AdminAccounts/Logout"
        let data = listObj [  ]
        let! res = asyncApiPost url data
        return box res :?> Result<Empty> }

    [<JavaScript>]
    let CreateAccount (account: Account)  = async {
        let url = "/api/AdminAccounts/CreateAccount"
        let data = listObj [ "account", box account;  ]
        let! res = asyncApiPost url data
        return box res :?> Result<Int32> }
        
    [<JavaScript>]
    let EditAccount (account: Account)  = async {
        let url = "/api/AdminAccounts/EditAccount"
        let data = listObj [ "account", box account;  ]
        let! res = asyncApiPost url data
        return box res :?> Result<Empty> }
        
    [<JavaScript>]
    let ListAccounts () = async {
        let url = "/api/AdminAccounts/ListAccounts"
        let! res = asyncApiPost url ""
        return box res :?> Result<Account[]> }

// SubscribersController
[<JavaScript>]
let ValidateAddress (address: Address)  = async {
    let url = "/api/Subscribers/ValidateAddress"
    let data = listObj [ "address", box address;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<AddressStatus> }
    
[<JavaScript>]
let SetSubscriber (subscriber: Subscriber)  = async {
    let url = "/api/Subscribers/SetSubscriber"
    let data = listObj [ "subscriber", box subscriber;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<AddressStatus> }
    
[<JavaScript>]
let GetSubscriber (number: String)  = async {
    let url = "/api/Subscribers/GetSubscriber"
    let data = listObj [ "number", box number;  ]
    let! res = asyncApiPostNoRedirect url data
    return box res :?> Result<Subscriber> }

[<JavaScript>]
let SearchSubscribers (accountId: Int32, name: string, number: string, address: string, page: Int32, size: Int32)  = async {
    let url = "/api/Subscribers/SearchSubscribers"
    let data = listObj [ "accountId", box accountId; "name", box name; "number", box number; "address", box address; "page", box page; "size", box size;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Subscriber[]> }
    
[<JavaScript>]
let AreAlertsEnabled ()  = async {
    let url = "/api/Subscribers/AreAlertsEnabled"
    let! res = asyncApiPostNoRedirect url null
    return box res :?> Result<bool> }
    
[<JavaScript>]
let SetNextTestSendAlerts (number: String)  = async {
    let url = "/api/Subscribers/SetNextTestSendAlerts"
    let data = listObj [ "number", box number;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let DeleteSubscriber (number: String)  = async {
    let url = "/api/Subscribers/DeleteSubscriber"
    let data = listObj [ "number", box number;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }
    
[<JavaScript>]
let EmailAllSubscribersAsAttachment(accountId: int)  = async {
    let url = "/api/Subscribers/EmailAllSubscribersAsAttachment"
    let data = listObj [ "accountId", box accountId;  ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }

    

// AlertsController
[<JavaScript>]
let SetAlertTarget (alertTarget: AlertTarget, number: String)  = async {
    let url = "/api/Alerts/SetAlertTarget"
    let data = listObj [ "alertTarget", box alertTarget; "number", box number; ]
    let! res = asyncApiPost url data
    return box res :?> Result<bool> }
    
[<JavaScript>]
let GetAlertTarget (number: String, alertTargetId : int)  = async {
    let url = "/api/Alerts/GetAlertTarget"
    let data = listObj [ "number", box number; "alertTargetId", box alertTargetId ]
    let! res = asyncApiPostNoRedirect url data
    return box res :?> Result<AlertTarget> }

[<JavaScript>]
let GetAlertTargets (number: String)  = async {
    let url = "/api/Alerts/GetAlertTargets"
    let data = listObj [ "number", box number;  ]
    let! res = asyncApiPostNoRedirect url data
    return box res :?> Result<AlertTarget[]> }
    
[<JavaScript>]
let DeleteAlertTarget (number: String, alertTargetId : int)  = async {
    let url = "/api/Alerts/DeleteAlertTarget"
    let data = listObj [ "number", box number; "alertTargetId", box alertTargetId ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }

//Cdrscontroller
[<JavaScript>]
let GetCdrs (crit: CdrSearchCriteria)  = async {
    let url = "/api/Cdrs/GetCdrs"
    let data = listObj [ "accountId", box Vars.AccountId; "crit", box crit; ]
    let! res = asyncApiPostNoRedirect url data
    return box res :?> Result<Cdr[]> }
    
[<JavaScript>]
let EmailAllCdrsAsAttachment(crit: CdrSearchCriteria)  = async {
    let url = "/api/Cdrs/EmailCdrsAsAttachment"
    let data = listObj [ "accountId", box Vars.AccountId; "crit", box crit; ]
    let! res = asyncApiPost url data
    return box res :?> Result<Empty> }