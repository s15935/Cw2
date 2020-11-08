using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;


namespace cw2
{
    class Program
    {
        static void Main(string[] args)
        {
            string csv;
            string results;
            string logs = "log.txt";
            string type;

            switch(args.Length)
            {
                case 1:
                    csv = args[0];
                    results = "result.txt";
                    type = "xml";
                    break;
                case 2:
                    csv = args[0];
                    results = args[1];
                    type = "xml";
                    break;
                case 3:
                    csv = args[0];
                    results = args[1];
                    type = args[2];
                    break;
                default:
                    csv = "dane.csv";
                    results = "result.txt";
                    type = "xml";
                    break;
            }

            try
            {
                if (File.Exists(csv))
                {
                    if (!Directory.Exists(results))
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Plik data.csv nie dostepny");
                using (StreamWriter sw = new StreamWriter(logs))
                {
                    sw.WriteLine("Brak/blad odczytu pliku data.csv " + csv + " " + DateTime.Now.ToString());
                }
                return;

            }
            catch (ArgumentException argument)
            {
                Console.WriteLine("Bledny argument");
                using (StreamWriter sw = new StreamWriter(logs))
                {
                    sw.WriteLine("Bledny argument " + results + " " + DateTime.Now.ToString() + "\n" + argument.Message.ToString());
                }
                return;
            }
            var data = File.ReadLines(csv);
            List<Student> students = new List<Student>();
            List<Student> errors = new List<Student>();
            var itmp = 0;
            var jtmp = new Student();
            Hashtable hash = new Hashtable();
            int students_count = 0;
            foreach (string line in data)
            {
                itmp = 0;
                var readed = line.Split(",");
                for (int i = 0; i < readed.Length; i++)
                {
                    if (readed[i].Length == 0)
                    {
                        continue;
                    }
                    itmp++;
                }

                Student stmp = new Student
                {
                    fname = readed[0],
                    lname = Regex.Replace(readed[1], @"[\d-]", ""),
                    studies = readed[2],
                    mode = readed[3],
                    indexNumber = readed[4],
                    birthdate = DateTime.Parse(readed[5]),
                    email = readed[6],
                    mothersName = readed[7],
                    fathersName = readed[8]
                };

                if (jtmp.indexNumber != readed[4] || itmp == 0)
                {
                    if (itmp != 9)
                    {
                        errors.Add(stmp);
                    }
                    else
                    {
                        students.Add(stmp);
                    }
                }
                else
                {
                    errors.Add(stmp);
                }
                jtmp = stmp;
            }

            using (StreamWriter sw = new StreamWriter(logs))
            {
                for (int i = 0; i < errors.Count; i++)
                {
                    Student person = errors[i];
                    sw.WriteLine(person.ToString());
                }
            }

            for (int i = 0; i < students.Count; i++)
            {
                if (hash.ContainsKey(students[i].studies))
                {
                    continue;
                }
                for (int j = 0; j < students.Count; j++)
                {
                    Student tmpP = students[j];
                    if (tmpP.studies == students[i].studies)
                    {
                        students_count++;
                    }
                }
                hash.Add(students[i].studies, students_count);
            }

            if (type.Contains("xml"))
            {
                CreateXml xml = new CreateXml();
                xml.CreateFile(students, hash, results);
            }
            else
            {
                CreateJson json = new CreateJson();
                json.CreateFile(students, hash, results);
            }
        }
    }

    class CreateJson
    {
        public University uni { get; set; }
        public void CreateFile(List<Student> student, Hashtable hash, string jsonPath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            CreateJson tmp = new CreateJson
            {
                uni = new University()
                {
                    creation = DateTime.Today.Date.ToString("d"),
                    author = "Stanislav Dizhechka",
                    students = student,
                    active_studies = hash
                }
            };
            File.WriteAllText(jsonPath + @"\result.json", JsonSerializer.Serialize(tmp, options));
            Console.WriteLine("File .json created");
        }
    }


    class CreateXml
    {
        public int[] students_count { get; set; }
        public void CreateFile(List<Student> students, Hashtable hash, string xmlPath)
        {
            XmlTextWriter xml_wr = new XmlTextWriter(xmlPath + @"\result.xml", System.Text.Encoding.UTF8);
            xml_wr.WriteStartDocument(true);
            xml_wr.Formatting = Formatting.Indented;
            xml_wr.Indentation = 2;
            xml_wr.WriteStartElement("University");
            xml_wr.WriteAttributeString("\ncreatedAt", DateTime.Today.Date.ToString("d"));
            xml_wr.WriteAttributeString("\nauthor", "Konstantsin Puchko");
            xml_wr.WriteStartElement("Studenci");
            for (int i = 0; i < students.Count; i++)
            {
                CreateNode(students[i].indexNumber, students[i].fname, students[i].lname, students[i].birthdate,
                           students[i].email, students[i].mothersName, students[i].fathersName, students[i].studies,
                           students[i].mode, xml_wr);
            }
            xml_wr.WriteEndElement();
            xml_wr.WriteStartElement("studies");

            foreach (DictionaryEntry entry in hash)
            {
                xml_wr.WriteStartElement("studies");
                xml_wr.WriteAttributeString("name", entry.Key.ToString());
                xml_wr.WriteAttributeString("numberOfStudents", entry.Value.ToString());
                xml_wr.WriteEndElement();
            }
            // ?
            xml_wr.WriteEndElement();
            xml_wr.WriteEndElement();
            // --- ?
            xml_wr.Close();
            Console.WriteLine("File .xml created");
        }

        private void CreateNode(string index, string fname, string lname, DateTime birthdate, string email, string mothersname,
                                string fathersname, string studies, string mode, XmlTextWriter xmlw)
        {
            xmlw.WriteStartElement("student");
            xmlw.WriteAttributeString("indexNumber", "s" + index.ToString());
            xmlw.WriteStartElement("fname");
            xmlw.WriteString(fname);
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("lname");
            xmlw.WriteString(lname);
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("birthdate");
            xmlw.WriteString(birthdate.Date.ToString("d"));
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("email");
            xmlw.WriteString(email);
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("mothersname");
            xmlw.WriteString(mothersname);
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("fathersname");
            xmlw.WriteString(fathersname);
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("studies");
            xmlw.WriteStartElement("name");
            xmlw.WriteString(studies.Replace(" dzienne", ""));
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("mode");
            xmlw.WriteString(mode);
            xmlw.WriteEndElement();
            xmlw.WriteEndElement();
            xmlw.WriteEndElement();
        }
    }

    class Student
    {
        [JsonPropertyName("indexNumber")]
        public string indexNumber { get; set; }
        [JsonPropertyName("fname")]
        public string fname { get; set; }
        [JsonPropertyName("lname")]
        public string lname { get; set; }
        [JsonPropertyName("birthdate")]
        public DateTime birthdate { get; set; }
        [JsonPropertyName("email")]
        public string email { get; set; }
        [JsonPropertyName("mothersName")]
        public string mothersName { get; set; }
        [JsonPropertyName("fathersName")]
        public string fathersName { get; set; }
        public string studies { get; set; }
        public string mode { get; set; }

        public string ToString()
        {
            return "indexNumber: " + indexNumber
                + " fname " + fname
                + " lname " + lname
                + " birthdate " + birthdate
                + " email " + email
                + " mothersname " + mothersName
                + " fathersname " + fathersName
                + " studies " + studies
                + " mode " + mode;
        }
    }

    class University
    {
        public string creation { get; set; }
        public string author { get; set; }
        public List<Student> students { get; set; }
        public Hashtable active_studies { get; set; }
    }


}
