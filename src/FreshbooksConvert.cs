﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using HastyAPI;
using Vooban.FreshBooks.DotNet.Api.Models;

namespace Vooban.FreshBooks.DotNet.Api
{
    /// <summary>
    /// Helper class to facilitate conversion from Freshbooks to the .NET world
    /// </summary>
    public static class FreshbooksConvert
    {
        /// <summary>
        /// Converts the string value representing a boolean in Freshbooks to a real <c>boolean</c> value.
        /// </summary>
        /// <param name="value">The Freshbooks value, which is 1 for <c>true</c> and 0 for <c>false</c>.</param>
        /// <returns>The boolean value corresponding to the Freshbooks string</returns>
        public static bool ToBoolean(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value == "1";
        }

        /// <summary>
        /// Converts the string value representing a double in Freshbooks to a real <c>double</c> value.
        /// </summary>
        /// <param name="value">The Freshbooks string value.</param>
        /// <returns>The double value corresponding to the Freshbooks string</returns>
        public static double? ToDouble(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }

            return null;
        }

        /// <summary>
        /// Converts the value representing a integer in Freshbooks to a real <c>integer</c> value.
        /// </summary>
        /// <param name="value">The Freshbooks string value.</param>
        /// <returns>The integer value corresponding to the Freshbooks object</returns>
        public static int? ToInt32(object value)
        {
            if (value == null)
                return null;

            return ToInt32(value.ToString());
        }

        /// <summary>
        /// Converts the string value representing a integer in Freshbooks to a real <c>integer</c> value.
        /// </summary>
        /// <param name="value">The Freshbooks string value.</param>
        /// <returns>The integer value corresponding to the Freshbooks string.</returns>
        public static int? ToInt32(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }

            return null;
        }

        /// <summary>
        /// Converts the value representing a percentage in Freshbooks to a <c>double</c> value.
        /// </summary>
        /// <param name="value">The Freshbooks string value.</param>
        /// <returns>The double value corresponding to the Freshbooks string percentage</returns>
        /// <remarks>Freshbooks percentage are represented as full number, but this methods returns the fractionnal.</remarks>
        public static double? ToPercentage(string value)
        {
            var result = ToDouble(value);

            if (result != null)
                return result/100;

            return null;
        }

        /// <summary>
        /// Converts the value representing a date in Freshbooks to a real <c>DateTime</c> value.
        /// </summary>
        /// <param name="value">The Freshbooks string value.</param>
        /// <returns>The date value corresponding to the Freshbooks object</returns>
        public static DateTime? ToDateTime(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            return null;
        }

        /// <summary>
        /// Convert the Freshbooks dynamic response to a <see cref="FreshbooksGetResponse{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of instance to return</typeparam>
        /// <param name="value">The dynamic response from Freshbooks</param>
        /// <returns>The prepolulated response status</returns>
        public static FreshbooksGetResponse<T> ToResponse<T>(dynamic value)
        {
            if (value == null)
                return null;

            return new FreshbooksGetResponse<T> {
                Status = value.response.status == "ok"
            };
        }

        /// <summary>
        /// Converts the dynamics Freshbooks response to a <see cref="FreshbooksPagedResponse{T}"/>
        /// </summary>
        /// <param name="value">The Freshbooks response</param>
        /// <returns>The converted paged response</returns>
        public static FreshbooksPagedResponse ToPagedResponse(dynamic value)
        {
            var responseFriendly = value.response as FriendlyDynamic;

            if (responseFriendly != null)
            {
                var keyValuePairsResponse = new Dictionary<string, object>(responseFriendly).ToList();
                var innerResponse = keyValuePairsResponse[2].Value as FriendlyDynamic;

                if (innerResponse != null)
                {
                    var pagingInfo = new Dictionary<string, object>(innerResponse);

                    var page = ToInt32(pagingInfo["page"]);
                    var itemPerPage = ToInt32(pagingInfo["per_page"]);
                    var totalPages = ToInt32(pagingInfo["pages"]);
                    var totalItems = ToInt32(pagingInfo["total"]);

                    return new FreshbooksPagedResponse
                    {
                        Status = value.response.status == "ok",
                        Page = page.HasValue ? page.Value : 1,
                        ItemPerPage = itemPerPage.HasValue ? itemPerPage.Value : 100,
                        TotalPages = totalPages.HasValue ? totalPages.Value : 1,
                        TotalItems = totalItems.HasValue ? totalItems.Value : 0
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the dynamics Freshbooks response to a <see cref="FreshbooksPagedResponse{T}" />
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IEnumerable{T}"/> that will be returned in the paged response.</typeparam>
        /// <param name="value">The Freshbooks response</param>
        /// <returns>
        /// The converted paged response
        /// </returns>
        public static FreshbooksPagedResponse<T> ToPagedResponse<T>(dynamic value) 
        {
            return new FreshbooksPagedResponse<T>(ToPagedResponse(value));           
        }

        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }
    }
}