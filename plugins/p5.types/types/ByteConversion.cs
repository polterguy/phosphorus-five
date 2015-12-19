/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using p5.exp;
using p5.core;
using p5.exp.exceptions;

namespace p5.types.types
{
    /// <summary>
    ///     Class helps converts from byte, and associated types, to object, and vice versa
    /// </summary>
    public static class ByteConversion
    {
        /// <summary>
        ///     Creates a byte from its string representation
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-object-value.byte", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_object_value_byte (ApplicationContext context, ActiveEventArgs e)
        {
            var strValue = e.Args.Value as string;
            if (strValue != null) {
                e.Args.Value = byte.Parse (strValue, CultureInfo.InvariantCulture);
            } else {
                throw new LambdaException (
                    "Don't know how to convert that to a byte",
                    e.Args, 
                    context);
            }
        }

        /// <summary>
        ///     Creates a byte array from its string/object representation
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-object-value.blob", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_object_value_blob (ApplicationContext context, ActiveEventArgs e)
        {
            // Checking if this is a string
            var strValue = e.Args.Value as string;
            if (strValue != null) {

                // Value is string, checking to see if we should decode from base64, or simply return raw bytes
                if (e.Args.GetChildValue ("decode", context, false)) {

                    // Caller specified he wanted to decode value from base64
                    e.Args.Value = Convert.FromBase64String (strValue);
                } else {

                    // No decoding here, returning raw bytes through UTF8 encoding
                    e.Args.Value = Encoding.UTF8.GetBytes (strValue);
                }
            } else {

                // Checking if value is a Node
                var nodeValue = e.Args.Value as Node;
                if (nodeValue != null) {

                    // Value is Node, converting to string before we convert to blob
                    strValue = e.Args.Get<string> (context);
                    e.Args.Value = Encoding.UTF8.GetBytes (strValue);
                } else {

                    // DateTime cannot be marshalled
                    if (e.Args.Value is DateTime)
                        e.Args.Value = ((DateTime)e.Args.Value).ToBinary ();
                    else
                        throw new LambdaException (
                            "Don't know how to convert that to a blob",
                            e.Args, 
                            context);
                }
            }
        }

        /// <summary>
        ///     Creates an sbyte from its string representation
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-object-value.sbyte", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_object_value_sbyte (ApplicationContext context, ActiveEventArgs e)
        {
            var strValue = e.Args.Value as string;
            if (strValue != null) {
                e.Args.Value = sbyte.Parse (strValue, CultureInfo.InvariantCulture);
            } else {
                throw new LambdaException (
                    "Don't know how to convert that to a sbyte",
                    e.Args, 
                    context);
            }
        }

        /// <summary>
        ///     Creates a char from its string representation
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-object-value.char", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_object_value_char (ApplicationContext context, ActiveEventArgs e)
        {
            var strValue = e.Args.Value as string;
            if (strValue != null) {
                e.Args.Value = char.Parse (strValue);
            } else {
                throw new LambdaException (
                    "Don't know how to convert that to a char",
                    e.Args, 
                    context);
            }
        }

        /// <summary>
        ///     Creates a string from a byte array
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-string-value.System.Byte[]", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_string_value_System_ByteBlob (ApplicationContext context, ActiveEventArgs e)
        {
            if (e.Args.GetChildValue ("encode", context, false))
                e.Args.Value = Convert.ToBase64String (e.Args.Get<byte[]> (context));
            else
                e.Args.Value = Encoding.UTF8.GetString (e.Args.Get<byte[]> (context));
        }

        /// <summary>
        ///     Returns the Hyperlisp type-name for the byte type
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-type-name.System.Byte", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_type_name_System_Byte (ApplicationContext context, ActiveEventArgs e)
        {
            e.Args.Value = "byte";
        }

        /// <summary>
        ///     Returns the Hyperlisp type-name for the byte array type
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-type-name.System.Byte[]", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_type_name_System_ByteBlob (ApplicationContext context, ActiveEventArgs e)
        {
            e.Args.Value = "blob";
        }

        /// <summary>
        ///     Returns the Hyperlisp type-name for the sbyte type
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-type-name.System.SByte", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_type_name_System_SByte (ApplicationContext context, ActiveEventArgs e)
        {
            e.Args.Value = "sbyte";
        }

        /// <summary>
        ///     Returns the Hyperlisp type-name for the char type
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.hyperlisp.get-type-name.System.Char", Protection = EventProtection.NativeClosed)]
        private static void p5_hyperlisp_get_type_name_System_Char (ApplicationContext context, ActiveEventArgs e)
        {
            e.Args.Value = "char";
        }
    }
}
