/*
 * Phosphorus Five, copyright 2014 - 2017, Thomas Hansen, thomas@gaiasoul.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3, as published by
 * the Free Software Foundation.
 *
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Phosphorus Five.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using System.IO;
using System.Linq;
using System.Globalization;
using p5.exp;
using p5.core;
using p5.exp.exceptions;

namespace p5.io.authorization.helpers
{
    /*
     * Helper class for authorization features in p5.io
     */
    static class AuthorizationHelper
    {
        /*
         * Verifies user is authorized reading from the specified file.
         */
        internal static void AuthorizeReadFile (ApplicationContext context, string filename, Node stack)
        {
            // Verifies filename is valid.
            if (!VerifySaneFileName (filename))
                throw new LambdaException (
                    string.Format ("Path '{0}' was not a valid file path", filename),
                    stack,
                    context);
            
            // Checking role of user, and invoking relevant check.
            if (context.Ticket.Role != "root") {
                
                // Making sure we do a lowers comparison.
                if (!UserHasReadAccessToFile (context, filename.ToLowerInvariant ()))
                    throw new LambdaException (
                        string.Format ("File '{0}' is off limits", filename),
                        stack,
                        context);
            }
        }

        /*
         * Verifies user is authorized modifying the specified file.
         */
        internal static void AuthorizeModifyFile (ApplicationContext context, string filename, Node stack)
        {
            // Verifies filename is valid.
            if (!VerifySaneFileName (filename))
                throw new LambdaException (
                    string.Format ("Path '{0}' was not a valid file path", filename),
                    stack,
                    context);

            // Checking role of user, and invoking relevant check.
            if (context.Ticket.Role != "root") {

                // Making sure we do a lowers comparison.
                if (!UserHasWriteAccessToFile (context, filename.ToLowerInvariant ()))
                    throw new LambdaException (
                        string.Format ("File '{0}' is off limits", filename),
                        stack,
                        context);
            }
        }
        
        /*
         * Verifies user is authorized reading from the specified folder.
         */
        internal static void AuthorizeReadFolder (ApplicationContext context, string foldername, Node stack)
        {
            // Verifies foldername is a valid foldername.
            if (!VerifySaneFolderName (foldername))
                throw new LambdaException (
                    string.Format ("Path '{0}' was not a valid folder path", foldername),
                    stack,
                    context);

            // Checking role of user, and invoking relevant check.
            if (context.Ticket.Role != "root") {

                // Making sure we do a lowers comparison.
                if (!UserHasReadAccessToFolder (context, foldername.ToLowerInvariant ()))
                    throw new LambdaException (
                        string.Format ("Folder '{0}' is off limits", foldername),
                        stack,
                        context);
            }

        }

        /*
         * Verifies user is authorized writing to the specified folder
         */
        internal static void AuthorizeModifyFolder (ApplicationContext context, string foldername, Node stack)
        {
            // Verifies foldername is valid.
            if (!VerifySaneFolderName (foldername))
                throw new LambdaException (
                    string.Format ("Folder '{0}' was not a valid folder path", foldername),
                    stack,
                    context);

            // Checking role of user, and invoking relevant check.
            if (context.Ticket.Role != "root") {

                // Making sure we do a lowers comparison.
                if (!UserHasReadAccessToFolder (context, foldername.ToLowerInvariant ()))
                    throw new LambdaException (
                        string.Format ("Folder '{0}' is off limits", foldername),
                        stack,
                        context);
            }
        }

        /*
         * Private helper methods below here.
         */

        /*
         * Verifies filename is sane.
         */
        private static bool VerifySaneFileName (string filename)
        {
            // Basic sanity check of given filename.
            if (string.IsNullOrEmpty (filename))
                return false;
            if (filename [0] != '/')
                return false;
            if (filename.Contains ("//"))
                return false;
            if (filename.Contains ("\\"))
                return false;
            if (filename.Contains (".."))
                return false;

            // Success.
            return true;
        }

        /*
         * Verifies foldername is sane.
         */
        private static bool VerifySaneFolderName (string foldername)
        {
            // A sane foldername has the same rules as a sane filename, except it must end with a "/".
            if (!VerifySaneFileName (foldername))
                return false;
            if (foldername [foldername.Length - 1] != '/')
                return false;

            // Success.
            return true;
        }
        
        /*
         * Helper used both for files and folders to check access rights.
         */
        private static bool CheckAccessRights (ApplicationContext context, string path, string operation, bool defaultValue)
        {
            // Creating our default access right.
            var hasAccess = defaultValue;

            // Retrieving all access right objects.
            var nodeResult = context.RaiseEvent ("p5.auth.access.list", new Node ());

            // Checking if we have any access objects at all.
            if (nodeResult.Count > 0) {

                // Getting children as list, such that we can more easily modify it.
                var access = nodeResult.Children.ToList ();

                // Removing all access right objects not relevant to current user, current path, and current operation type.
                access.RemoveAll (ix => ix.Name != "*" && ix.Name != context.Ticket.Role);
                access.RemoveAll (ix => ix ["p5.io.allow-" + operation] == null && ix ["p5.io.deny-" + operation] == null);
                access.RemoveAll (ix => !path.StartsWithEx (ix [0].Get<string> (context)));

                // Checking if we still have some access right object(s).
                if (access.Count > 0) {

                    // Sorting remaining access rights on their value.
                    access.Sort (delegate (Node lhs, Node rhs) {
                        
                        // First doing a path comparison.
                        var retVal = string.Compare (lhs [0].Get<string> (context), rhs [0].Get<string> (context), true, CultureInfo.InvariantCulture);
                        
                        /*
                         * If the paths were similar, we make sure all asterix (*) roles are sorted as before any special role rights.
                         * We do this such that a specifically mentioned role can override the value for an asterix role declaration.
                         */
                        if (retVal == 0) {
                            if (lhs.Name == "*" && rhs.Name != "*")
                                retVal = -1;
                            else if (lhs.Name != "*" && rhs.Name == "*")
                                retVal = 1;
                        }
                        return retVal;
                    });

                    /*
                     * Looping through any remaining access rights, to see if that modifies our return value.
                     */
                    foreach (var idxAccess in access) {
                        if (idxAccess [0].Name == "p5.io.allow-" + operation)
                            hasAccess = true;
                        else if (idxAccess [0].Name == "p5.io.deny-" + operation)
                            hasAccess = false;
                    }
                }
            }

            // Returns access to caller.
            return hasAccess;
        }
        
        /*
         * Verifies non-root user has access to reading file.
         * The default is "true".
         */
        private static bool UserHasReadAccessToFile (ApplicationContext context, string path)
        {
            // Verifying file is underneath authenticated user's folder, if it is underneath "/users/" folder.
            if (path.StartsWithEx ("/users/") && !path.StartsWithEx (string.Format ("/users/{0}/", context.Ticket.Username)))
                return false;

            // Verify all database files are safe.
            var dbPath = context.RaiseEvent (".p5.config.get", new Node (".p5.config.get", ".p5.data.path")) [0].Get (context, "/db/");
            if (path.StartsWithEx (dbPath))
                return false;
            
            // Verify *.config is safe.
            if (Path.GetExtension (path) == ".config")
                return false;
            
            // Verifying "auth" file is safe.
            if (path == GetAuthFile (context).ToLower ())
                return false;
                
            // Returning value of access rights check.
            return CheckAccessRights (context, path, "read", true);
        }
        
        /*
         * Verifies non-root user has access to reading folder.
         */
        private static bool UserHasReadAccessToFolder (ApplicationContext context, string path)
        {
            // Shielding other user's folders here.
            if (path.StartsWithEx ("/users/") && 
                path.Length > "/users/".Length && 
                !path.StartsWithEx (string.Format ("/users/{0}/", context.Ticket.Username)))
                return false;

            // Verify all database folders are safe.
            var dbPath = context.RaiseEvent (".p5.config.get", new Node (".p5.config.get", ".p5.data.path")) [0].Get (context, "/db/");
            if (path.StartsWithEx (dbPath))
                return false;
            
            // Returning value of access rights check.
            return CheckAccessRights (context, path, "read", true);
        }
        
        /*
         * Verifies non-root user has access to writing file.
         * The default is "false"
         */
        private static bool UserHasWriteAccessToFile (ApplicationContext context, string path)
        {
            // Checking if this is user's file.
            if (path.StartsWithEx (string.Format ("/users/{0}/", context.Ticket.Username)))
                return true;

            // Checking if this is a common folder.
            if (path.StartsWithEx ("/common/"))
                return true;
            
            // Returning value of access rights check.
            return CheckAccessRights (context, path, "write", false);
        }

        /*
         * Verifies non-root user has access to modify folder.
         */
        private static bool UserHasWriteAccessToFolder (ApplicationContext context, string path)
        {
            // Checking if this is user's file.
            if (path.StartsWithEx ("/users/") && path.StartsWithEx (string.Format ("/users/{0}/", context.Ticket.Role)))
                return true;
            
            // Checking if this is a common folder.
            if (path.StartsWithEx ("/common/"))
                return true;
            
            // Returning value of access rights check.
            return CheckAccessRights (context, path, "write", false);
        }

        /*
         * Returns the filename of our "auth" file.
         */
        public static string GetAuthFile (ApplicationContext context)
        {
            return context.RaiseEvent (".p5.auth.get-auth-file").Get<string> (context);
        }
    }
}