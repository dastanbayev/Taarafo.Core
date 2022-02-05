﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using Xeptions;

namespace Taarafo.Core.Tests.Unit.Services.Foundations.Profiles
{
    public class AlreadyExistsProfileException : Xeption
    {
        public AlreadyExistsProfileException(Exception innerException)
            : base(message: "Profile with the same id already exists.", innerException)
        { }
    }
}
