/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Web.UI;
using System.Collections;
using System.Collections.Generic;
using p5.exp;
using p5.core;
using p5.ajax.core;
using p5.ajax.widgets;
using p5.exp.exceptions;

namespace p5.web.widgets
{
    /// <summary>
    ///     Class encapsulating properties of widgets
    /// </summary>
    public class WidgetProperties : BaseWidget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="p5.web.widgets.WidgetProperties"/> class
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="manager">PageManager owning this instance</param>
        public WidgetProperties (ApplicationContext context, PageManager manager)
            : base (context, manager)
        { }

        #region [ -- Widget property getters and setters -- ]

        /// <summary>
        ///     Returns properties and/or attributes requested by caller as children nodes
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "get-widget-property", Protection = EventProtection.LambdaClosed)]
        public void get_widget_property (ApplicationContext context, ActiveEventArgs e)
        {
            // Making sure we clean up and remove all arguments passed in after execution
            using (new p5.core.Utilities.ArgsRemover (e.Args, true)) {

                // Looping through all widget IDs given by caller
                foreach (var idxWidget in FindWidgets<Widget> (context, e.Args, "get-widget-property")) {

                    // Looping through all properties requested by caller
                    foreach (var nameNode in e.Args.Children.Where (ix => ix.Name != "").ToList ()) {

                        // Checking if this is a generic attribute, or a specific property
                        switch (nameNode.Name) {
                        case "visible":
                            CreatePropertyReturn (e.Args, nameNode, idxWidget, idxWidget.Visible);
                            break;
                        case "invisible-element":
                            CreatePropertyReturn (e.Args, nameNode, idxWidget, idxWidget.InvisibleElement);
                            break;
                        case "element":
                            CreatePropertyReturn (e.Args, nameNode, idxWidget, idxWidget.Element);
                            break;
                        case "has-id":
                            CreatePropertyReturn (e.Args, nameNode, idxWidget, idxWidget.HasID);
                            break;
                        case "render-type":
                            CreatePropertyReturn (e.Args, nameNode, idxWidget, idxWidget.RenderType.ToString ());
                            break;
                        default:
                            if (!string.IsNullOrEmpty (nameNode.Name))
                                CreatePropertyReturn (e.Args, nameNode, idxWidget);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Sets properties and/or attributes of web widgets
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "set-widget-property", Protection = EventProtection.LambdaClosed)]
        public void set_widget_property (ApplicationContext context, ActiveEventArgs e)
        {
            // Looping through all widget IDs given by caller
            foreach (var idxWidget in FindWidgets<Widget> (context, e.Args, "set-widget-property")) {

                // Looping through all properties requested by caller
                foreach (var valueNode in e.Args.Children.Where (ix => ix.Name != "")) {
                    switch (valueNode.Name) {
                    case "visible":
                        idxWidget.Visible = valueNode.GetExValue<bool> (context);
                        break;
                    case "invisible-element":
                        idxWidget.InvisibleElement = valueNode.GetExValue<string> (context);
                        if (!idxWidget.Visible)
                            idxWidget.ReRender ();
                        break;
                    case "element":
                        idxWidget.Element = valueNode.GetExValue<string> (context);
                        idxWidget.ReRender ();
                        break;
                    case "has-id":
                        idxWidget.HasID = valueNode.GetExValue<bool> (context);
                        idxWidget.ReRender ();
                        break;
                    case "render-type":
                        idxWidget.RenderType = (Widget.RenderingType) Enum.Parse (typeof (Widget.RenderingType), valueNode.GetExValue<string> (context));
                        idxWidget.ReRender ();
                        break;
                    case "id":
                        var oldID = idxWidget.ID;
                        idxWidget.ID = valueNode.GetExValue<string> (context);
                        if (oldID != idxWidget.ID) {
                            Manager.WidgetAjaxEventStorage.ChangeKey1 (context, oldID, idxWidget.ID);
                            Manager.WidgetLambdaEventStorage.ChangeKey2 (oldID, idxWidget.ID);
                        }
                        break;
                    default:
                        idxWidget [valueNode.Name.StartsWith ("\\") ? valueNode.Name.Substring(1) : valueNode.Name] = valueNode.GetExValue<string> (context);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Removes the properties and/or attributes of web widgets
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "delete-widget-property", Protection = EventProtection.LambdaClosed)]
        public void delete_widget_property (ApplicationContext context, ActiveEventArgs e)
        {
            if (e.Args.Value == null || e.Args.Children.Count == 0)
                return; // Nothing to do here ...

            // Looping through all widgets supplied by caller
            foreach (var widget in FindWidgets<Widget> (context, e.Args, "delete-widget-property")) {

                // Looping through each property to remove
                foreach (var nameNode in e.Args.Children.Where (ix => ix.Name != "")) {

                    // Verifying property can be removed
                    switch (nameNode.Name) {
                    case "visible":
                    case "invisible-element":
                    case "element":
                    case "has-id":
                    case "has-name":
                    case "render-type":
                        throw new LambdaException ("Cannot remove property '" + nameNode.Name + "' of widget", e.Args, context);
                    default:
                        widget.RemoveAttribute (nameNode.Name.StartsWith ("\\") ? nameNode.Name.Substring(1) : nameNode.Name);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Lists all existing properties for given web widget
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "list-widget-properties", Protection = EventProtection.LambdaClosed)]
        public void list_widget_properties (ApplicationContext context, ActiveEventArgs e)
        {
            // Making sure we clean up and remove all arguments passed in after execution
            using (new p5.core.Utilities.ArgsRemover (e.Args, true)) {

                // Looping through all widgets
                foreach (var widget in FindWidgets<Widget> (context, e.Args, "list-widget-properties")) {

                    // Creating our "return node" for currently handled widget
                    Node curNode = e.Args.Add (widget.ID).LastChild;

                    // First listing "static properties"
                    if (!widget.Visible)
                        curNode.Add ("visible", false);
                    curNode.Add ("element", widget.Element);
                    if (!widget.HasID)
                        curNode.Add ("has-id", false);

                    // Then the generic attributes
                    foreach (var idxAtr in widget.AttributeKeys) {

                        // Dropping the Tag property and all events, except events that are "JavaScript declarations"
                        if (idxAtr == "Element" || ((idxAtr.StartsWith ("on") || idxAtr.StartsWith ("_on")) && widget [idxAtr] == "common_event_handler"))
                            continue;
                        curNode.Add (idxAtr, widget [idxAtr]);
                    }
                }
            }
        }

        #endregion

        #region [ -- Private helper methods -- ]

        /*
         * Helper for [get-widget-property], creates a return value for one property
         */
        private static void CreatePropertyReturn (Node node, Node nameNode, Widget widget, object value = null)
        {
            var propertyName = nameNode.Name.StartsWith ("\\") ? nameNode.Name.Substring (1) : nameNode.Name;
            // Checking if widget has the attribute, if it doesn't, we don't even add any return nodes at all, to make it possible
            // to separate widgets which has the property, but no value, (such as the selected property on checkboxes for instance),
            // and widgets that does not have the property at all
            if (value == null && !widget.HasAttribute (propertyName))
                return;

            node.FindOrCreate (widget.ID).Add (nameNode.Name).LastChild.Value = value == null ? 
                widget [propertyName] : 
                value;
        }
        #endregion
    }
}
