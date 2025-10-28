// Models/TtsRequest.cs
using Microsoft.AspNetCore.Mvc;

namespace TtsApiCaller.Models;

public class TtsRequest
{
    // [FromForm] 속성을 사용하여 form 데이터에서 값을 바인딩하도록 지정합니다.
    [FromForm(Name = "text")]
    public string Text { get; set; }

    [FromForm(Name = "audioFile")]
    public IFormFile AudioFile { get; set; }
}