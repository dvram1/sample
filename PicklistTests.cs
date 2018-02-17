using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atlas.Service.MediaFrame.Common;
using NUnit.Framework;
using Microsoft.Practices.Unity;
using Atlas.Service.Contracts;
using Atlas.Service.MediaFrame;
using FakeItEasy;

namespace Atlas.Tests.Common
{
    [TestFixture]
    class PicklistTests 
    {
        private FieldList GetFields()
        {
            Field field1 = new Field() { SchemaName = "atlas", EntityName = "Asset", PublicName = "PN1", FieldName = "Field1", Type = FieldType.String, IsExpensive = false, IsPrivate = false };
            Field field2 = new Field() { SchemaName = "atlas", EntityName = "Asset", PublicName = "PN2", FieldName = "Field2", Type = FieldType.String, IsExpensive = false, IsPrivate = false };
            Field field3 = new Field() { SchemaName = "atlas", EntityName = "Asset", PublicName = "PN3", FieldName = "Field3", Type = FieldType.String, IsExpensive = true, IsPrivate = false };
            Field field4 = new Field() { SchemaName = "atlas", EntityName = "Asset", PublicName = "PN4", FieldName = "Field4", Type = FieldType.String, IsExpensive = false, IsPrivate = true };
            Field field5 = new Field() { SchemaName = "atlas", EntityName = "Asset", PublicName = "PN5", FieldName = "Field5", Type = FieldType.String, IsExpensive = false, IsPrivate = false };

            return new FieldList { field1, field2, field3, field4, field5 };
        }

        [Test]
        public void TestNormalize()
        {
            FieldList fieldList = GetFields();
            PickList picklist = new PickList("Field1+Field2");
  
            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 2);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 2);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2");
        }

        [Test]
        public void TestNormalizewithAnotherConstructor()
        {
            FieldList fieldList = GetFields();
            IEnumerable<string> strings = new string[]{ "Field1", "Field2" };
            PickList picklist = new PickList(strings);

            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 2);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 2);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2");
        }

        [Test]
        public void TestNormalizewithEmptyPicklistvalue()
        {
            FieldList fieldList = GetFields();
            PickList picklist = new PickList("");
            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2+atlas.Asset.Field3+atlas.Asset.Field4+atlas.Asset.Field5");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 5);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 5);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2+Field3+Field4+Field5");
        }

        [Test]
        public void TestNormalizewithFQPicklist()
        {
            FieldList fieldList = GetFields(); 
            PickList picklist = new PickList("atlas.Asset.Field1+atlas.Asset.Field2");
            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 2);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 2);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2");
        }

        [Test]
        public void TestNormalizewithEntityFieldNamePicklist()
        {
            FieldList fieldList = GetFields();
            PickList picklist = new PickList("Asset.Field1+Asset.Field2");
            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 2);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 2);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2");
        }

        [Test]
        public void TestNormalizewithAllFieldsinPicklist()
        {
            FieldList fieldList = GetFields();
            PickList picklist = new PickList("Field1+Field2+" + PickList.AllFields);
            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2+atlas.Asset.Field3+atlas.Asset.Field4+atlas.Asset.Field5");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 5);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 5);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2+Field3+Field4+Field5");
        }

        [Test]
        public void TestNormalizewithInexpensiveinPicklist()
        {
            FieldList fieldList = GetFields();
            PickList picklist = new PickList("Field1+Field2+" + PickList.Inexpensive);
            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2+atlas.Asset.Field5");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 3);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 3);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2+Field5");
        }


        [Test]
        public void TestNormalizePickList()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickList("Field1+Field2", fieldList);
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
        }

        [Test]
        public void TestNormalizePickListwithEmptyPicklistvalue()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickList("", fieldList);
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1+atlas.Asset.Field2+atlas.Asset.Field3+atlas.Asset.Field4+atlas.Asset.Field5");
        }

        [Test]
        public void TestNormalizePickListwithFQPicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickList("atlas.Asset.Field1+atlas.Asset.Field2", fieldList);
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
        }

        [Test]
        public void TestNormalizePickListwithEntityFieldNamePicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickList("Asset.Field1+Asset.Field2", fieldList);
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
        }

        [Test]
        public void TestNormalizePickListwithAllFieldsinPicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickList("Field1+Field2+" + PickList.AllFields, fieldList);
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1+atlas.Asset.Field2+atlas.Asset.Field3+atlas.Asset.Field4+atlas.Asset.Field5");
        }

        [Test]
        public void TestNormalizePickListwithInexpensiveinPicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickList("Field1+Field2+" + PickList.Inexpensive, fieldList);
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1+atlas.Asset.Field2+atlas.Asset.Field5");
        }

        [Test]
        public void TestAddFieldField()
        {
            FieldList fieldList = GetFields();
            PickList picklist = new PickList("Field1+Field2");

            picklist.Normalize(fieldList);
            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 2);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 2);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2");

            Field newfield = new Field() { SchemaName = "atlas", EntityName = "Asset", PublicName = "NewField", FieldName = "NewField", Type = FieldType.String, IsExpensive = false, IsPrivate = false };
            picklist.AddField(newfield);

            Assert.AreEqual(picklist.FullyQualifiedPickList, "atlas.Asset.Field1+atlas.Asset.Field2+atlas.Asset.NewField");
            Assert.IsTrue(picklist.FullyQualifiedPickListCollection.Count() == 3);
            Assert.IsTrue(picklist.BarePickListCollection.Count() == 3);
            Assert.AreEqual(picklist.BarePickList, "Field1+Field2+NewField");
        }

        [Test]
        public void TestNormalizePickListField()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickListField(fieldList,"Field1");
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNormalizePickListFieldwithEmptyPicklistvalue()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickListField(fieldList, "");
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1");
        }

        [Test]
        public void TestNormalizePickListFieldwithFQPicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickListField(fieldList, "atlas.Asset.Field1");
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1");
        }

        [Test]
        public void TestNormalizePickListFieldwithEntityFieldNamePicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickListField(fieldList, "Asset.Field1");
            Assert.AreEqual(normalizedPickList, "atlas.Asset.Field1");
        }

        [Test]
        public void TestNormalizePickListFieldwithAllFieldsinPicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickListField(fieldList, PickList.AllFields);
            Assert.AreEqual(normalizedPickList, "All");
        }

        [Test]
        public void TestNormalizePickListFieldwithInexpensiveinPicklist()
        {
            FieldList fieldList = GetFields();
            string normalizedPickList = PickList.NormalizePickListField(fieldList, PickList.Inexpensive);
            Assert.AreEqual(normalizedPickList, "Inexpensive");
        }


        [Test]
        public void TestContains()
        {
            FieldList fieldList = GetFields();
            IEnumerable<string> strings = new string[] { "Field1", "Field2" };
            PickList picklist = new PickList(strings);
            
            Assert.IsTrue(picklist.Contains("Field1"));
            Assert.IsFalse(picklist.Contains("Field25"));
        }

    }
}
