// Program.cs

// 1. �츮�� �� ���� ���� Ŭ������ ����ϰڴٰ� �����մϴ�.
using TtsApiCaller.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// �������������������� ���⿡ �ڵ� �߰� ��������������������
// TtsService Ŭ������ HttpClient�� ����� �� �ֵ��� ����ϰ� �⺻ �ּҸ� �����մϴ�.
builder.Services.AddHttpClient<TtsService>(client =>
{
    // ������ ������̽� �����̽� �ּҸ� ��Ȯ�ϰ� �Է��ؾ� �մϴ�.
    client.BaseAddress = new Uri("https://limkang-ttstest.hf.space");
    client.Timeout = TimeSpan.FromSeconds(300);
});
// �������������������� �ڵ� �߰� �Ϸ� ��������������������

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