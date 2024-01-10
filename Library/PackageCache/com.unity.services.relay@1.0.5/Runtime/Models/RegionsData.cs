//-----------------------------------------------------------------------------
// <auto-generated>
//     This file was generated by the C# SDK Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.Relay.Http;



namespace Unity.Services.Relay.Models
{
    /// <summary>
    /// RegionsData model
    /// </summary>
    [Preserve]
    [DataContract(Name = "RegionsData")]
    public class RegionsData
    {
        /// <summary>
        /// Creates an instance of RegionsData.
        /// </summary>
        /// <param name="regions">An array of regions where Relay servers might be available.</param>
        [Preserve]
        public RegionsData(List<Region> regions)
        {
            Regions = regions;
        }

        /// <summary>
        /// An array of regions where Relay servers might be available.
        /// </summary>
        [Preserve]
        [DataMember(Name = "regions", IsRequired = true, EmitDefaultValue = true)]
        public List<Region> Regions{ get; }
    
        /// <summary>
        /// Formats a RegionsData into a string of key-value pairs for use as a path parameter.
        /// </summary>
        /// <returns>Returns a string representation of the key-value pairs.</returns>
        internal string SerializeAsPathParam()
        {
            var serializedModel = "";

            if (Regions != null)
            {
                serializedModel += "regions," + Regions.ToString();
            }
            return serializedModel;
        }

        /// <summary>
        /// Returns a RegionsData as a dictionary of key-value pairs for use as a query parameter.
        /// </summary>
        /// <returns>Returns a dictionary of string key-value pairs.</returns>
        internal Dictionary<string, string> GetAsQueryParam()
        {
            var dictionary = new Dictionary<string, string>();

            return dictionary;
        }
    }
}