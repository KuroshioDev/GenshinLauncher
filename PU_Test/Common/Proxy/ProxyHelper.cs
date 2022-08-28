using Microsoft.Win32;
using PU_Test.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace PU_Test.Common.Proxy
{
    internal static class ProxyHelper
    {

        public static ProxyConfig GetCurrentProxy()
        {
            ProxyConfig proxyConfig = new ProxyConfig(false, "127.0.0.1");

            try
            {
                using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings"))
                {
                    if (regkey.GetValue("ProxyEnable").ToString() == "1")
                    {
                        proxyConfig.ProxyEnable = true;
                    }
                    object ps = regkey.GetValue("ProxyServer");
                    if (ps != null)
                    {
                        proxyConfig.ProxyServer = ps.ToString();
                    }
                    else
                    {
                        ps = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return proxyConfig;
        }

        public static void Clear_Proxy()
        {
            using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
            {
                try
                {
                    regkey.SetValue("ProxyEnable", 0);
                    regkey.DeleteValue("ProxyServer");
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                }
            }
        }

        public class ProxyController
        {
            ProxyServer proxyServer;
            ExplicitProxyEndPoint explicitEndPoint;
            private string port;
            private string fakeHost;

            public ProxyController(string port, string host)
            {
                this.port = port;
                this.fakeHost = host;
            }

            private bool IsRun;

            public bool _IsRun
            {
                get { return proxyServer.ProxyRunning; }
                set { IsRun = value; }
            }


            public void Start()
            {
                if (GetCurrentProxy().ProxyEnable)
                {
                   Clear_Proxy();
                }

                proxyServer = new ProxyServer();
                proxyServer.CertificateManager.EnsureRootCertificate();


                proxyServer.BeforeRequest += OnRequest;
                proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
                if (String.IsNullOrEmpty(port))
                {
                    port = 11451.ToString(); ;
                }

                explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, int.Parse(port), true);

                explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;

                proxyServer.AddEndPoint(explicitEndPoint);
                proxyServer.Start();


                foreach (var endPoint in proxyServer.ProxyEndPoints)
                    Console.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                        endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);

                proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
                proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

            }



            public void Stop()
            {
                try
                {
                    explicitEndPoint.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;
                    proxyServer.BeforeRequest -= OnRequest;
                    proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;


                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (proxyServer.ProxyRunning)
                    {
                        proxyServer.Stop();

                    }
                    else
                    {
                    }

                }

            }

            public void UninstallCertificate()
            {

                proxyServer.CertificateManager.RemoveTrustedRootCertificate();
                proxyServer.CertificateManager.RemoveTrustedRootCertificateAsAdmin();


            }


            private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
            {
                string hostname = e.WebSession.Request.RequestUri.Host;
                if (hostname.EndsWith(".yuanshen.com") |
                   hostname.EndsWith(".hoyoverse.com") |
                   hostname.EndsWith(".mihoyo.com") | hostname.EndsWith(fakeHost))
                {
                    e.DecryptSsl = true;
                }
                else
                {

                    e.DecryptSsl = false;
                }
            }


            private async Task OnRequest(object sender, SessionEventArgs e)
            {
                string hostname = e.WebSession.Request.RequestUri.Host;
                if (hostname.EndsWith(".yuanshen.com") |
                   hostname.EndsWith(".hoyoverse.com") |
                   hostname.EndsWith(".mihoyo.com"))
                {
                    string oHost = e.WebSession.Request.RequestUri.Host;
                    e.HttpClient.Request.Url = e.HttpClient.Request.Url.Replace(oHost, fakeHost);
                }
            }

            // Allows overriding default certificate validation logic
            private Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
            {
                e.IsValid = true;
                return Task.CompletedTask;
            }

        }
    }

}
