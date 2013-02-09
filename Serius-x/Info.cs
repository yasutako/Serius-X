using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Serius
{
    [DataContract]
    class Info
    {
        public static String address = AppDomain.CurrentDomain.BaseDirectory + "/info.inf";
        [DataMember]
        private String l_project;
        public String latest_project
        {
            get {return l_project;}
            set
            {
                 l_project = value;
                 changed = true;
            }
        }
        [DataMember]
        private String m_address;
        public String mongo_address
        {
            get { return m_address; }
            set
            {
                m_address = value;
                changed = true;
            }
        }
        [DataMember]
        private String n_version;
        public String node_version
        {
            get { return n_version; }
            set
            {
                n_version = value;
                changed = true;
            }
        }
        [DataMember]
        private String n_dev_version;
        public String node_dev_version
        {
            get { return n_dev_version; }
            set
            {
                n_dev_version = value;
                changed = true;
            }
        }
        [DataMember]
        private String n_inspector_exist;
        public String node_inspector_exist
        {
            get { return n_inspector_exist; }
            set
            {
                n_inspector_exist = value;
                changed = true;
            }
        }
        [DataMember]
        private String c_address;
        public String chrome_address
        {
            get { return c_address; }
            set
            {
                c_address = value;
                changed = true;
            }
        }
        [DataMember]
        private List<String> projects = new List<String>();
        public void add_project(String address)
        {
            if (projects == null) projects = new List<String>();
            int n;
            if ((n = projects.IndexOf(address)) != -1)
            {
                projects.RemoveAt(n);
            }
            projects.Add(address);
            changed = true;
        }
        public void remove_project(String address) {
            projects.Remove(address);
            changed = true;
        }
        [DataMember]
        public List<String> shell_recomends = new List<String>();
        public void add_shell_recomend(String address)
        {
            if (shell_recomends == null) shell_recomends = new List<String>();
            int n;
            if ((n = shell_recomends.IndexOf(address)) != -1)
            {
                shell_recomends.RemoveAt(n);
            }
            shell_recomends.Add(address);
            if (shell_recomends.Count >= 16) shell_recomends.RemoveAt(0);
            changed = true;
        }
        public bool changed = false;
        public static Info read()
        {
            Info info = null;
            FileInfo fi = new FileInfo(address);
            if (fi.Exists == false) return new Info() { changed = true };
            using (FileStream fs = new FileStream(address, FileMode.Open))
            {
                var serializer = new DataContractJsonSerializer(typeof(Info));
                info = (Info)serializer.ReadObject(fs);
            }
            return info;
        }
        public void write()
        {
            if (changed == false) return;
            using (FileStream fs = new FileStream(address, FileMode.Create))
            {
                var serializer = new DataContractJsonSerializer(typeof(Info));
                serializer.WriteObject(fs, this);
            }
        }
    }
}
