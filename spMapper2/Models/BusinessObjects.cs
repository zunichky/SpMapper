using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;

namespace spMapper2
{
    #region Object Collections
    class Permissions
    {
        private bool _inheritsPermissions;
        public bool InheritsPermissions
        {
            get
            {
                return _inheritsPermissions;
            }
        }

        private Collection<PermissionRole> _permissions;
        public Collection<PermissionRole> Items
        {
            get
            {
                return _permissions;
            }
        }

        private void ProcessPermissions(XmlElement data)
        {
            _permissions = new Collection<PermissionRole>();
            XmlNodeList permissionRoles = data.SelectNodes("permissionRole");
            foreach (XmlElement permissionRole in permissionRoles)
            {
                PermissionRole newPermissionRole = new PermissionRole(permissionRole);
                _permissions.Add(newPermissionRole);
            }
        }

        public Permissions(XmlElement data, Permissions parentPermissions)
        {
            _inheritsPermissions = data.GetAttribute("inherits") == "true" ? true : false;
            if (!_inheritsPermissions  || parentPermissions == null)
            {
                ProcessPermissions(data);
            }
            else
            {
                _permissions = parentPermissions.Items;
            }
        }
    }
    #endregion

    #region Base Objects
    class SecurableObject
    {
        private Site _parent;
        public Site Parent
        {
            get
            {
                return _parent;
            }
        }

        private Permissions _permissions;
        public Permissions ActivePermissions
        {
            get
            {
                return _permissions;
            }
        }

        public SecurableObject(XmlElement data, Site parent, Permissions parentPermissions)
        {
            _parent = parent;

            XmlElement permissions = (XmlElement)data.SelectSingleNode("permissions");
            _permissions = new Permissions(permissions, parentPermissions);
        }
    }
    #endregion

    #region Single Objects
    class Folder : SecurableObject
    {
        private Collection<Folder> _folders;
        public Collection<Folder> Folders
        {
            get
            {
                return _folders;
            }
        }

        private DateTime _lastModifiedDate;
        public DateTime LastModifiedDate
        {
            get
            {
                return _lastModifiedDate;
            }
        }

        private DateTime _newestSubItemModifiedDate;
        public DateTime NewestSubItemModifiedDate
        {
            get
            {
                return _newestSubItemModifiedDate;
            }
        }

        public int ItemCount { get; private set; }
        public string Title { get; private set; }

        public Folder(XmlElement data, Site parent, Permissions parentPermissions) : base(data, parent, parentPermissions)
        {
            ItemCount = string.IsNullOrEmpty(data.GetAttribute("itemcount")) ? 0 : int.Parse(data.GetAttribute("itemcount"));
            Title = data.GetAttribute("title");

            XmlNodeList items = data.SelectNodes("item");
            foreach (XmlElement item in items)
            {
                DateTime lastModified = DateTime.Parse(item.GetAttribute("datemodified"));
                if (lastModified > _lastModifiedDate) _lastModifiedDate = lastModified;
            }

            if (_lastModifiedDate > _newestSubItemModifiedDate) _newestSubItemModifiedDate = _lastModifiedDate;

            XmlNodeList subFolders = data.SelectNodes("folder");
            _folders = new Collection<Folder>();

            foreach (XmlElement subFolder in subFolders)
            {
                Folder newFolder = new Folder(subFolder, parent, ActivePermissions);
                _folders.Add(newFolder);

                if (newFolder.NewestSubItemModifiedDate > _newestSubItemModifiedDate) _newestSubItemModifiedDate = newFolder.NewestSubItemModifiedDate;
            }
        }
    }

    class List : SecurableObject
    {
        private Collection<Folder> _folders;
        public Collection<Folder> Folders
        {
            get
            {
                return _folders;
            }
        }

        private bool _isDocumentLibrary;
        public bool IsDocumentLibrary
        {
            get
            {
                return _isDocumentLibrary;
            }
        }

        private DateTime _lastModifiedDate;
        public DateTime LastModifiedDate
        {
            get
            {
                return _lastModifiedDate;
            }
        }

        public bool Hidden { get; private set; }
        public int ItemCount { get; private set; }
        public string Title { get; private set; }

        public List(XmlElement data, Site parent, Permissions parentPermissions) : base(data, parent, parentPermissions)
        {
            Hidden = data.GetAttribute("hidden") == "True" ? true : false;
            ItemCount = string.IsNullOrEmpty(data.GetAttribute("itemcount")) ? 0 : int.Parse(data.GetAttribute("itemcount"));
            Title = data.GetAttribute("title");

            _isDocumentLibrary = data.GetAttribute("basetype") == "DocumentLibrary" ? true : false;

            if (_isDocumentLibrary)
            {
                XmlNodeList subFolders = data.SelectNodes("items/folder");
                _folders = new Collection<Folder>();

                foreach (XmlElement subFolder in subFolders)
                {
                    Folder newFolder = new Folder(subFolder, parent, ActivePermissions);
                    _folders.Add(newFolder);

                    if (newFolder.NewestSubItemModifiedDate > _lastModifiedDate) _lastModifiedDate = newFolder.NewestSubItemModifiedDate;
                }
            }
            else
            {
                XmlNodeList items = data.SelectNodes("items/item");
                foreach (XmlElement item in items)
                {
                    DateTime lastModified = DateTime.Parse(item.GetAttribute("datemodified"));
                    if (lastModified > _lastModifiedDate) _lastModifiedDate = lastModified;
                }
            }
        }
    }

    class PermissionRole
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private string _account;
        public string Account
        {
            get
            {
                return _account;
            }
        }

        private StringCollection _roles;
        public StringCollection Roles
        {
            get
            {
                return _roles;
            }
        }

        public PermissionRole(XmlElement data)
        {
            _name = data.GetAttribute("name");
            _account = data.GetAttribute("account");

            _roles = new StringCollection();

            string[] roles = data.GetAttribute("roles").Split(',');
            foreach (string role in roles)
            {
                _roles.Add(role);
            }
        }
    }

    class Site : SecurableObject
    {
        private Collection<List> _lists;
        public Collection<List> Lists
        {
            get
            {
                return _lists;
            }
        }

        private Collection<Site> _subSites;
        public Collection<Site> SubSites
        {
            get
            {
                return _subSites;
            }
        }

        public string Title { get; private set; }
        public string Url { get; private set; }

        public Site(XmlElement data, Site parent, Permissions parentPermissions) : base(data, parent, parentPermissions)
        {
            Title = data.GetAttribute("title");
            Url = data.GetAttribute("url");

            _lists = new Collection<List>();

            XmlNodeList lists = data.SelectNodes("list");
            foreach (XmlElement list in lists)
            {
                List newList = new List(list, this, ActivePermissions);
                _lists.Add(newList);
            }

            _subSites = new Collection<Site>();
            XmlNodeList subSites = data.SelectNodes("site");
            foreach (XmlElement subSite in subSites)
            {
                Site newSite = new Site(subSite, this, ActivePermissions);
                _subSites.Add(newSite);
            }
        }
    }
    #endregion
}
