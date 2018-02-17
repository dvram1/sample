using System;
using System.Collections.Generic;
using System.Linq;
using Atlas.Service.Contracts;

namespace Atlas.Service.MediaFrame.Common
{
    public class PickList
    {
        /// <summary>
        /// A constant string value representing a group containing all the 
        /// non-expensive properties.
        /// </summary>
        public const string Inexpensive = "Inexpensive";

        /// <summary>
        /// A constant string value representing a group containing all the 
        /// non-private properties.
        /// </summary>
        public const string AllFields = "All";

        public const char PickListSeparator = '+';

        private readonly string _pickList;
        private string _barePickList;
        private IEnumerable<string> _barePickListCollection;
        private string _fullyQualifiedPickList;
        private IEnumerable<string> _fullyQualifiedPickListCollection;
        private readonly object _lockObject = new object();

        public PickList(string pickList)
        {
            _pickList = pickList;
        }

        public PickList(IEnumerable<string> pickList)
        {
            if (pickList != null && pickList.Any())
            {
                _pickList = string.Join(PickListSeparator.ToString(), pickList);
            }
        }

        public IEnumerable<string> FullyQualifiedPickListCollection
        {
            get
            {
                if (_fullyQualifiedPickListCollection == null)
                {
                    throw new ApplicationException("PickList has not been normalized");
                }

                return _fullyQualifiedPickListCollection;
            }
        }

        public IEnumerable<string> BarePickListCollection
        {
            get
            {
                if (_barePickListCollection == null)
                {
                    throw new ApplicationException("PickList has not been normalized");
                }

                return _barePickListCollection;
            }
        }

        public string FullyQualifiedPickList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fullyQualifiedPickList))
                {
                    throw new ApplicationException("PickList has not been normalized");
                }

                return _fullyQualifiedPickList;
            }
        }

        public string BarePickList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_barePickList))
                {
                    throw new ApplicationException("PickList has not been normalized");
                }

                return _barePickList;
            }
        }

        public void Normalize(IEnumerable<Field> entityFields)
        {
            // materialize the entityFields collection if it is an enumerator rather than a collection.
            if (!(entityFields is IList<Field>))
            {
                entityFields = entityFields.ToArray();
            }

            lock (_lockObject)
            {
                _fullyQualifiedPickList = NormalizePickList(_pickList, entityFields);
                _fullyQualifiedPickListCollection = FullyQualifiedPickList.Split(PickListSeparator);
                _barePickListCollection = FullyQualifiedPickListCollection.Select(x => DequalifyPickListField(x)).ToArray();
                _barePickList = string.Join(PickListSeparator.ToString(), BarePickListCollection);
            }
        }

        private static string DequalifyPickListField(string pickListField)
        {
            string dequalified = pickListField.Substring(pickListField.LastIndexOf('.') + 1);
            return dequalified;
        }

        public void AddField(Field field)
        {
            lock (_lockObject)
            {
                if (!FullyQualifiedPickListCollection.Contains(field.Name, StringComparer.CurrentCultureIgnoreCase))
                {
                    _barePickListCollection = _barePickListCollection.Union(new string[] { field.FieldName }).ToArray();
                    _barePickList = string.Join(PickListSeparator.ToString(), _barePickList, field.FieldName);
                    _fullyQualifiedPickListCollection = _fullyQualifiedPickListCollection.Union(new string[] { field.Name }).ToArray();
                    _fullyQualifiedPickList = string.Join(PickListSeparator.ToString(), _fullyQualifiedPickList, field.Name);
                }
            }
        }

        public static string NormalizePickList(string pickList, IEnumerable<Field> fields)
        {
            pickList = string.IsNullOrWhiteSpace(pickList) ? AllFields : pickList;

            IEnumerable<string> notNormalizedPicks =
                pickList
                    .Split('+')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.CurrentCultureIgnoreCase)
                    .Select(x => x.Trim());

            HashSet<string> normalizedPickList = new HashSet<string>();

            foreach (string pick in notNormalizedPicks)
            {
                if (StringComparer.CurrentCultureIgnoreCase.Equals(pick, AllFields))
                {
                    IEnumerable<string> allFieldNames = fields
                        .Where(x => !(x.FieldName == MarkerHarmonizedName.MappedId && x.EntityName == "Marker"))
                        .Select(x => x.Name)
                        .Distinct(StringComparer.CurrentCultureIgnoreCase);
                    return string.Join("+", allFieldNames);
                }
                else if (StringComparer.CurrentCultureIgnoreCase.Equals(pick, Inexpensive))
                {
                    IEnumerable<string> inexpensiveFields =
                        fields
                            .Where(x => !x.IsExpensive 
                                && !x.IsPrivate 
                                && !(x.FieldName == MarkerHarmonizedName.MappedId && x.EntityName == "Marker"))
                            .Select(x => x.Name)
                            .Distinct(StringComparer.CurrentCultureIgnoreCase);

                    foreach (string inexpensiveField in inexpensiveFields)
                    {
                        normalizedPickList.Add(inexpensiveField);
                    }
                }
                else
                {
                    string normalizedField = NormalizePickListField(fields, pick);
                    normalizedPickList.Add(normalizedField);
                }
            }

            return string.Join("+", normalizedPickList);
        }

        public static string NormalizePickListField(IEnumerable<Field> fields, string field)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new ArgumentException("field cannot be null/empty", "field");
            }

            if (StringComparer.CurrentCultureIgnoreCase.Equals(field, Inexpensive) || StringComparer.CurrentCultureIgnoreCase.Equals(field, AllFields))
            {
                return field;
            }

            string[] pick = field.Split('.');
            if (pick.Length < 3)
            {
                IEnumerable<Field> matches = fields;
                if (pick.Length == 2)
                {
                    matches = matches.Where(x =>
                        StringComparer.CurrentCultureIgnoreCase.Equals(x.EntityName, pick.First()) ||
                        StringComparer.CurrentCultureIgnoreCase.Equals(x.SchemaName, pick.First()));
                }

                matches = matches.Where(x =>
                    StringComparer.CurrentCultureIgnoreCase.Equals(x.FieldName, pick.Last()));

                if (matches.Any())
                {
                    if (matches.Count() == 1)
                    {
                        field = matches.First().Name;
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("[Pick List] Ambiguous field: '{0}'", field));
                    }
                }
                else
                {
                    throw new ArgumentException(string.Format("[Pick List] Field not found: '{0}'", field));
                }
            }
            else if (pick.Length > 3)
            {
                throw new ArgumentException(string.Format("[Pick List] Invalid field: '{0}'; field must be specified as <schema>.<entity>.<field>.", field));
            }
            else if (!fields.Any(x => StringComparer.CurrentCultureIgnoreCase.Equals(field, x.Name)))
            {
                // mediaframe name may be specified instead of atlas name; try to resolve
                var matches = fields.Where(x =>
                    StringComparer.CurrentCultureIgnoreCase.Equals(x.SchemaName, pick[0]) &&
                    StringComparer.CurrentCultureIgnoreCase.Equals(x.EntityName, pick[1]) &&
                    StringComparer.CurrentCultureIgnoreCase.Equals(x.FieldName, pick[2]));

                if (matches.Count() == 1)
                {
                    field = matches.First().Name;
                }
                else if (matches.Count() > 1)
                {
                    throw new ArgumentException(string.Format("[Pick List] Ambiguous field: '{0}'", field));
                }
                else
                {
                    throw new ArgumentException(string.Format("[Pick List] Field not found: '{0}'", field));
                }
            }

            return field;
        }

        public bool Contains(string value)
        {
            string[] currentValues = _pickList.Split(PickListSeparator);
            string[] checkValues = value.Split(PickListSeparator);

            if (currentValues.Intersect(checkValues).Any())
            {
                return true;
            }

            return false;
        }
    }
}
