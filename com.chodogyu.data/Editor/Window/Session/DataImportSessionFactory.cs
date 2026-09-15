using System;
using System.Reflection;
using CDG.Core.Results;
using CDG.Data;
using UnityEngine;

namespace CDG.Data.Editor
{
    /// <summary>
    /// EditorWindow에서 선택한 DataTableAsset의 실제 Entry 타입을 확인하고
    /// 해당 타입에 맞는 Generic DataImportSession을 생성합니다.
    /// </summary>
    internal static class DataImportSessionFactory
    {
        /// <summary>
        /// 지정한 DataTableAsset을 처리할 Import Session을 생성합니다.
        /// </summary>
        internal static Result<IDataImportSession> Create(UnityEngine.Object targetAsset)
        {
            if (targetAsset == null)
            {
                throw new ArgumentNullException(nameof(targetAsset));
            }

            if (!DataTableAssetTypeUtility.TryGetEntryType(
                targetAsset,
                out Type entryType))
            {
                return CreateFailure(
                    $"'{targetAsset.GetType().FullName}' 타입은 DataTableAsset<T>가 아닙니다.");
            }

            Type sessionType = typeof(DataImportSession<>)
                .MakeGenericType(entryType);

            try
            {
                object session = Activator.CreateInstance(
                    sessionType,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic,
                    null,
                    new object[]
                    {
                        targetAsset
                    },
                    null);

                if (session is not IDataImportSession importSession)
                {
                    return CreateFailure(
                        $"'{entryType.FullName}' 타입의 Import Session을 생성하지 못했습니다.");
                }

                return Result<IDataImportSession>.Success(
                    importSession);
            }
            catch (TargetInvocationException exception)
            {
                string message = exception.InnerException != null
                    ? exception.InnerException.Message
                    : exception.Message;

                return CreateFailure(
                    $"Import Session 생성 중 오류가 발생했습니다. {message}");
            }
            catch (ArgumentException exception)
            {
                return CreateFailure(
                    $"Import Session 생성 중 오류가 발생했습니다. {exception.Message}");
            }
            catch (MissingMethodException exception)
            {
                return CreateFailure(
                    $"Import Session 생성자를 찾을 수 없습니다. {exception.Message}");
            }
        }

        private static Result<IDataImportSession> CreateFailure(string message)
        {
            return Result<IDataImportSession>.Failure(
                new ResultError(
                    DataErrorCodes.ImportFailed,
                    message));
        }
    }
}