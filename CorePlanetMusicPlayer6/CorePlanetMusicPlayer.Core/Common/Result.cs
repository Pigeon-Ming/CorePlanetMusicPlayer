using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Common
{
    /// <summary>
    /// 用于标识操作结果
    /// </summary>
    public sealed class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; }

        private Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }

    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, T value, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(false, default, errorMessage);
        }
    }
}
