// Program.cs

// 1. 우리가 곧 만들 서비스 클래스를 사용하겠다고 선언합니다.
using TtsApiCaller.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ 여기에 코드 추가 ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
// TtsService 클래스가 HttpClient를 사용할 수 있도록 등록하고 기본 주소를 설정합니다.
builder.Services.AddHttpClient<TtsService>(client =>
{
    // 본인의 허깅페이스 스페이스 주소를 정확하게 입력해야 합니다.
    client.BaseAddress = new Uri("https://limkang-ttstest.hf.space");
    client.Timeout = TimeSpan.FromSeconds(300);
});
// ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ 코드 추가 완료 ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();