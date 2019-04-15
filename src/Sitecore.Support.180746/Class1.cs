using System.Security.Principal;
using LightLDAP.Caching;
using Sitecore.Diagnostics;
using Sitecore.StringExtensions;

namespace LightLDAP.Support
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using LightLDAP.DataSource;
    using LightLDAP.Notification;
    using LightLDAP.Notification.Events;

    public class SitecoreADRoleProvider : LightLDAP.SitecoreADRoleProvider
    {
        private RolesCache rolesCache;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            // get initialized DirectoryNotificationProvider instance
            MembershipResolver resolver = (MembershipResolver) this.GetType().BaseType
                .GetField("resolver", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            rolesCache =
                this.GetType().BaseType.GetField("cache", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(this) as RolesCache;

            PartialDataSource partialDatasource = resolver.Source;
            DirectoryNotificationProvider notificationProvider = (DirectoryNotificationProvider) partialDatasource
                .GetType().GetField("notificationProvider", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(partialDatasource);


            if (notificationProvider != null)
            {
                // cleanup the value of the ObjectCategoryAdded event and set the custom handler which performs the check if a role exists 
                notificationProvider.ObjectCategoryAdded +=
                    new EventHandler<ObjectCategoryAddedEventArgs>(this.OnRoleAdded);
                notificationProvider.ObjectCategoryModified +=
                    new EventHandler<ObjectCategoryModifiedEventArgs>(this.OnRoleModified);
            }
        }

        private void OnRoleModified(object sender, ObjectCategoryModifiedEventArgs e)
        {
            Log.Debug(
                "DIAGNOSTICS\n\tOnRoleModified. Object Name:{0}\n\tGroupType:{1}\n\tFullOUEntryPath:{2}".FormatWith(e.UserName,
                    e.GroupType, e.FullOUEntryPath), this);
            if (!string.IsNullOrEmpty(e.GroupType))
            {
                this.rolesCache.Clear();
            }
        }

        // the method checks if the role exists within the scope defined by custom filter and if yes invokes the original one to add the role to the Role cache.
        private void OnRoleAdded(object sender, ObjectCategoryAddedEventArgs e)
        {
            Log.Debug(
                "DIAGNOSTICS\n\tOnRoleAdded. Object Name:{0}\n\tGroupType:{1}\n\tFullOUEntryPath:{2}".FormatWith(e.UserName,
                    e.GroupType, e.FullOUEntryPath), this);
            if (!string.IsNullOrEmpty(e.GroupType))
            {
                this.rolesCache.Clear();
            }
        }

    }
}

