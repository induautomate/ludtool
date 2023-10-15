using RockwellSoftware.RSLogix5000ServicesDotNet;
using RSLogix5000ServicesLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixUploadDownloadTool
{
    /// <summary>
    /// Manages the lifetime of the services.
    /// </summary>
    internal class LogixServicesLifetimeManager
    {
        private RockwellSoftware.RSLogix5000ServicesDotNet.LogixServices _services;

        /// <summary>
        /// Gets the services.
        /// </summary>
        public ILogixServices Services { get; private set; }

        /// <summary>
        /// Creates a LogixServicesLifetimeManager
        /// </summary>
        /// <param name="iServices">Services</param>
        /// <param name="serviceInstance">Service Instance</param>
        public LogixServicesLifetimeManager(ILogixServices iServices, RockwellSoftware.RSLogix5000ServicesDotNet.LogixServices serviceInstance)
        {
            Services = iServices;
            _services = serviceInstance;
        }

        /// <summary>
        /// Releases the resources.
        /// </summary>
        public void Release()
        {
            _services.Release();

            //The Release doesn't seem to kill the actual process, so we have to do it
            //manually. If there is a better way, I would love to know about it.
            var p = Process.GetProcessesByName("RSLogix5000Services");
            var r = p.FirstOrDefault(proc => proc.Id > Process.GetCurrentProcess().Id);

            r?.Kill();
        }
    }
}
