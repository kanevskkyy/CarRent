﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CarRent.Application.Interfaces.Services;

public interface IImageUploaderService
{
    Task<string> UploadAsync(
        Stream stream, 
        string fileName, 
        CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}