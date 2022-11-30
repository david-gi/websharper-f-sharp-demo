namespace Website

open IntelliFactory.Html
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Sitelets
open Api


module Skin =
    open System.Web

    let TemplateLoadFrequency =
        #if DEBUG
        Content.Template.PerRequest
        #else
        Content.Template.WhenChanged
        #endif

    type Page =
        {
            Title : string
            Body : list<Content.HtmlElement>
        }

    let MainTemplate =
        let path = HttpContext.Current.Server.MapPath("~/Main.html")
        Content.Template<Page>(path, TemplateLoadFrequency)
            .With("title", fun x -> x.Title)
            .With("body", fun x -> x.Body)

    let WithTemplate title body : Content<Action> =
        Content.WithTemplate MainTemplate <| fun context ->
            {
                Title = title
                Body = body context
            }

module Site =

    let ( => ) text url =
        A [HRef url] -< [Text text]
                        
    let LoginPage =
        Skin.WithTemplate "Login" <| fun ctx ->            
            [
                Div [ new Demo911App.Account.LoginControl() ]                 
            ]
               
    let LogoutPage =
        Skin.WithTemplate "Logout" <| fun ctx ->            
            [
                Div [ new Demo911App.Account.LogoutControl() ]
            ]
                
    let LoginWrapper =
        let lControl = Div[Id "LoginOverlay"; Style "display:none; position:absolute; width:100%; height:100%; top:0; left:0; padding:10% 20%;";]
                        -< [ Div[Style ("position:absolute; width:100%; height:100%; top:0; left:0; background-color:#777;z-index:99990;" + 
                                        "opacity:0.6; filter: alpha(opacity=60); -moz-opacity: 0.6;")] ]
                        -< [ Div[Style "position:absolute; z-index:99999;"] -< [new Demo911App.Account.LoginControl()];]
        lControl
        
    let DashboardPage =
        Skin.WithTemplate "Dashboard" <| fun ctx ->
            [
                LoginWrapper
                Div [ new Demo911App.Subscribers.SubscriberControl() ]
                Br[]
                Div [ new Demo911App.Account.DashboardControl() ]
            ]
                
    let SubscriberPage =
        Skin.WithTemplate "Addresses" <| fun ctx ->
            [
                LoginWrapper          
                Div [new Demo911App.Subscribers.SubscriberControl()]
                Br[]
                Div [ new Demo911App.SearchSubscribers.SearchSubscribersControl() ]
            ]

    let BulkUploadPage =
        Skin.WithTemplate "Bulk Upload" <| fun ctx ->
            [
                LoginWrapper          
                Div [new Demo911App.BulkUpload.BulkUploadControl()]
                Br[]
                Div [ new Demo911App.SearchSubscribers.SearchSubscribersControl() ]
            ]
            
    let ApiKeysPage =
        Skin.WithTemplate "API Keys" <| fun ctx ->
            [
                LoginWrapper
                Div [ new Demo911App.ApiKeys.ApiKeysControl() ]
            ]
            
    let UsersPage =
        Skin.WithTemplate "Users" <| fun ctx ->
            [
                LoginWrapper
                Div [ new Demo911App.Users.UsersControl() ]
            ]
            
    let CdrsPage =
        Skin.WithTemplate "CDRs" <| fun ctx ->
            [
                LoginWrapper          
                Div [ new Demo911App.Cdrs.CdrsControl() ]
            ]
            
    let GatewaysPage =
        Skin.WithTemplate "Gateways" <| fun ctx ->
            [
                LoginWrapper
                //Div [ new Demo911App.Gateways.GatewaysControl() ]
            ]
            
    let AlertsPage =
        Skin.WithTemplate "Alerts" <| fun ctx ->
            [
                LoginWrapper
                Div [ new Demo911App.Alerts.AlertsControl() ]
            ]

    let AdminLoginPage =
        Skin.WithTemplate "Admin Login" <| fun ctx ->            
            [
                Div [ new Demo911App.Admin.LoginControl() ]
            ]

    let AdminLogoutPage =
        Skin.WithTemplate "Admin Logout" <| fun ctx ->            
            [
                Div [ new Demo911App.Admin.LogoutControl() ]
            ]

    let AdminDashboardPage =
        Skin.WithTemplate "Admin Dashboard" <| fun ctx ->            
            [
                Div [ new Demo911App.Admin.AccountsControl() ]
            ]

    let Main =
        Sitelet.Sum [
            Sitelet.Content "/Login" Api.Action.Login LoginPage
            Sitelet.Content "/Logout" Api.Action.Logout LogoutPage
            Sitelet.Content "/" Api.Action.Dashboard DashboardPage
            Sitelet.Content "/Addresses" Api.Action.Addresses SubscriberPage
            Sitelet.Content "/BulkUpload" Api.Action.BulkUpload BulkUploadPage
            Sitelet.Content "/ApiKeys" Api.Action.ApiKeys ApiKeysPage
            Sitelet.Content "/Users" Api.Action.Users UsersPage
            Sitelet.Content "/Gateways" Api.Action.Gateways GatewaysPage
            Sitelet.Content "/Alerts" Api.Action.Alerts AlertsPage
            Sitelet.Content "/Cdrs" Api.Action.Cdrs CdrsPage
            Sitelet.Shift "/Admin" <|
                Sitelet.Sum [
                    Sitelet.Content "/Login" Api.Action.AdminLogin AdminLoginPage
                    Sitelet.Content "/Logout" Api.Action.AdminLogout AdminLogoutPage
                    Sitelet.Content "/" Api.Action.AdminDashboard AdminDashboardPage
                ]
        ]

type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = []

[<assembly: WebsiteAttribute(typeof<Website>)>]
do ()
