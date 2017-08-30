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
    public override void Initialize(string name, NameValueCollection config)
    {
      base.Initialize(name, config);

      // get initialized DirectoryNotificationProvider instance
      MembershipResolver resolver = (MembershipResolver)this.GetType().BaseType.GetField("resolver", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
      PartialDataSource partialDatasource = resolver.Source;
      DirectoryNotificationProvider notificationProvider = (DirectoryNotificationProvider)partialDatasource.GetType().GetField("notificationProvider", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(partialDatasource);


      if (notificationProvider != null)
      {
        // cleanup the value of the ObjectCategoryAdded event and set the custom handler which performs the check if a role exists 
        notificationProvider.GetType().GetField("ObjectCategoryAdded", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(notificationProvider, null);
        notificationProvider.ObjectCategoryAdded += new EventHandler<ObjectCategoryAddedEventArgs>(this.OnRoleAddedWithCheck);


        // set patched DirectoryNotificationProvider instance back.
        partialDatasource.GetType().GetField("notificationProvider", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(partialDatasource, notificationProvider);
        resolver.GetType().GetProperty("Source").SetValue(resolver, partialDatasource);
        this.GetType().BaseType.GetField("resolver", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, resolver);
      }
    }

    // the method checks if the role exists within the scope defined by custom filter and if yes invokes the original one to add the role to the Role cache.
    private void OnRoleAddedWithCheck(object sender, ObjectCategoryAddedEventArgs e)
    {
      if (RoleExists(e.UserName))
      {
        MethodInfo mI = this.GetType().BaseType.GetMethod("OnRoleAdded", BindingFlags.NonPublic | BindingFlags.Instance);
        object[] param = new[] { sender, e };
        mI.Invoke(this, param);
      }
    }

  }
}

