// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Network.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Add, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "LoadBalancerProbeConfig", SupportsShouldProcess = true), OutputType(typeof(PSLoadBalancer))]
    public partial class AddAzureRmLoadBalancerProbeConfigCommand : NetworkBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The reference of the load balancer resource.",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public PSLoadBalancer LoadBalancer { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Name of the probe.")]
        public string Name { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The protocol of the end point. If 'Tcp' is specified, a received ACK is required for the probe to be successful. If 'Http' or 'Https' is specified, a 200 OK response from the specifies URI is required for the probe to be successful.",
            ValueFromPipelineByPropertyName = true)]
        [PSArgumentCompleter(
            "Http",
            "Tcp",
            "Https"
        )]
        public string Protocol { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The port for communicating the probe. Possible values range from 1 to 65535, inclusive.",
            ValueFromPipelineByPropertyName = true)]
        public int Port { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The interval, in seconds, for how frequently to probe the endpoint for health status. Typically, the interval is slightly less than half the allocated timeout period (in seconds) which allows two full probes before taking the instance out of rotation. The default value is 15, the minimum value is 5.",
            ValueFromPipelineByPropertyName = true)]
        public int IntervalInSeconds { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The number of probes where if no response, will result in stopping further traffic from being delivered to the endpoint. This values allows endpoints to be taken out of rotation faster or slower than the typical times used in Azure.",
            ValueFromPipelineByPropertyName = true)]
        public int ProbeCount { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The number of consecutive successful or failed probes in order to allow or deny traffic from being delivered to this endpoint. After failing the number of consecutive probes equal to this value, the endpoint will be taken out of rotation and require the same number of successful consecutive probes to be placed back in rotation.",
            ValueFromPipelineByPropertyName = true)]
        [AllowNull]
        public int? ProbeThreshold { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The URI used for requesting health status from the VM. Path is required if a protocol is set to http. Otherwise, it is not allowed. There is no default value.",
            ValueFromPipelineByPropertyName = true)]
        public string RequestPath { get; set; }


        public override void Execute()
        {

            var existingProbe = this.LoadBalancer.Probes.SingleOrDefault(resource => string.Equals(resource.Name, this.Name, System.StringComparison.CurrentCultureIgnoreCase));
            if (existingProbe != null)
            {
                throw new ArgumentException("Probe with the specified name already exists");
            }

            // Probes
            if (this.LoadBalancer.Probes == null)
            {
                this.LoadBalancer.Probes = new List<PSProbe>();
            }

            var vProbes = new PSProbe();

            vProbes.Protocol = this.Protocol;
            vProbes.Port = this.Port;
            vProbes.IntervalInSeconds = this.IntervalInSeconds;
            vProbes.NumberOfProbes = this.ProbeCount;
            vProbes.ProbeThreshold = this.ProbeThreshold;
            vProbes.RequestPath = this.RequestPath;
            vProbes.Name = this.Name;
            var generatedId = string.Format(
                "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Network/loadBalancers/{2}/{3}/{4}",
                this.NetworkClient.NetworkManagementClient.SubscriptionId,
                this.LoadBalancer.ResourceGroupName,
                this.LoadBalancer.Name,
                "Probes",
                this.Name);
            vProbes.Id = generatedId;

            this.LoadBalancer.Probes.Add(vProbes);
            WriteObject(this.LoadBalancer, true);
        }
    }
}
