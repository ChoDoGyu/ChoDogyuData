using System;
using System.Collections.Generic;
using CDG.Core.Results;
using CDG.Data;
using CDG.Data.Editor;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Data.Tests.Editor
{
    public sealed class DataImportSessionFactoryTests
    {
        private readonly List<ScriptableObject> createdObjects =
            new List<ScriptableObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (ScriptableObject createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        createdObject);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void Create_DirectDataTableAsset_ReturnsExpectedSession()
        {
            DirectTestAsset target =
                CreateObject<DirectTestAsset>();

            Result<IDataImportSession> result =
                DataImportSessionFactory.Create(target);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Value.EntryType,
                Is.EqualTo(typeof(TestEntry)));

            Assert.That(
                result.Value.TargetAsset,
                Is.SameAs(target));
        }

        [Test]
        public void Create_IndirectDataTableAsset_ReturnsExpectedSession()
        {
            IndirectTestAsset target =
                CreateObject<IndirectTestAsset>();

            Result<IDataImportSession> result =
                DataImportSessionFactory.Create(target);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Value.EntryType,
                Is.EqualTo(typeof(TestEntry)));

            Assert.That(
                result.Value.TargetAsset,
                Is.SameAs(target));
        }

        [Test]
        public void Create_NonDataTableAsset_ReturnsImportFailed()
        {
            PlainScriptableObject target =
                CreateObject<PlainScriptableObject>();

            Result<IDataImportSession> result =
                DataImportSessionFactory.Create(target);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(
                result.Error.Code,
                Is.EqualTo(DataErrorCodes.ImportFailed));
        }

        [Test]
        public void Create_NullTarget_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DataImportSessionFactory.Create(null));
        }

        private T CreateObject<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            createdObjects.Add(instance);
            return instance;
        }

        [Serializable]
        private sealed class TestEntry : IDataEntry
        {
            [SerializeField]
            private string id;

            [SerializeField]
            private int value;

            public string Id => id;
        }

        private sealed class DirectTestAsset : DataTableAsset<TestEntry>
        {
        }

        private abstract class IntermediateTestAsset<T> : DataTableAsset<T> where T : IDataEntry
        {
        }

        private sealed class IndirectTestAsset : IntermediateTestAsset<TestEntry>
        {
        }

        private sealed class PlainScriptableObject : ScriptableObject
        {
        }
    }
}