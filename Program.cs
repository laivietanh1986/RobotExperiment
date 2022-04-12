using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RobotDb>(opt => opt.UseInMemoryDatabase("RoboExperiment"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// create a map 
var map = new Map(100,100);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/robots", async (RobotDb db) =>
    await db.Robots.ToListAsync());

app.MapGet("/robots/{id}", async (int id, RobotDb db) =>
    await db.Robots.FindAsync(id)
        is Robot robot
            ? Results.Ok(robot)
            : Results.NotFound());

app.MapPost("/robots", async (Robot robot, RobotDb db) =>
{
    db.Robots.Add(robot);
    await db.SaveChangesAsync();

    return Results.Created($"/robots/{robot.Id}", robot);
});

app.MapPut("/robots/{id}/commands", async (int id, string commands, RobotDb db) =>
{
 
    var robot = await db.Robots.FindAsync(id);

    if (robot is null) return Results.NotFound();

   var strCommands = commands.Split(',').ToList();

    foreach (var commandStr in strCommands)
    {
        Command command;
        if(Enum.TryParse(commandStr, out command)){
            System.Console.WriteLine(command);
           robot.DoAction(command,map);
        }
    }

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/robots/{id}", async (int id, RobotDb db) =>
{
    if (await db.Robots.FindAsync(id) is Robot robot)
    {
        db.Robots.Remove(robot);
        await db.SaveChangesAsync();
        return Results.Ok(robot);
    }

    return Results.NotFound();
});


app.Run();

class Robot
{
    public int Id { get; set; }   
    public int PositionX { get; set; }   
    public int PositionY { get; set; }   
    public Direction direction {get;set;}
    public Robot (){
        PositionX = 0;
        PositionY=0;
    }
    public bool IsNotValidMove(Map map) {
         switch (direction)
        {
            case Direction.North:{
               int newPositionY = PositionY +1;
               return newPositionY > map.MapDemensionY;
             
            }
            case Direction.West:{
               int newPositionX = PositionX -1;
               return newPositionX< 0;
             
            }
            case Direction.South:{
                int newPositionY = PositionY - 1;
               return newPositionY < 0;
             
            }
            case Direction.East:{
               int newPositionX = PositionX +1;
               return newPositionX > map.MapDemensionX;
            
            }
            default:break;
        }
        return true;      
    }
    public void DoAction(Command command,Map map){
        switch (command)
        {
            case Command.Advance:{
                if(!IsNotValidMove(map)){
                    Move();                    
                }                 
                break;
            }
            case Command.Left:{
                TurnLeft();                    
                break;
            }
            case Command.Right:{
                TurnRight();                    
                break;
            }                
            default:
                break;
        }
    }
    public void Move(){
        switch (direction)
        {
            case Direction.North:{
               PositionY++;
                break;    
            }
            case Direction.West:{
               PositionX--;
                break;    
            }
            case Direction.South:{
                PositionY--;
                break;    
            }
            case Direction.East:{
                PositionX++;
                break;    
            }
            default:break;
        }
    }
    public void TurnLeft(){
        switch (direction)
        {
            case Direction.North:{
                direction = Direction.West;
                break;    
            }
            case Direction.West:{
                direction = Direction.South;
                break;    
            }
            case Direction.South:{
                direction = Direction.East;
                break;    
            }
            case Direction.East:{
                direction = Direction.North;
                break;    
            }
            default:break;
        }

    }
    public void TurnRight(){
        switch (direction)
        {
            case Direction.North:{
                direction = Direction.East;
                break;    
            }
            case Direction.East:{
                direction = Direction.South;
                break;    
            }            
            case Direction.South:{
                direction = Direction.West;
                break;    
            }
            case Direction.West:{
                direction = Direction.North;
                break;    
            }
            default:break;
        }
        
    }
 
    
}

class RobotDb : DbContext
{
    public RobotDb(DbContextOptions<RobotDb> options)
        : base(options) { }

    public DbSet<Robot> Robots => Set<Robot>();
}
class Map {
    public int MapDemensionX { get; set; }
    public int MapDemensionY { get; set; }   
    public Map(int x , int y ) {
        this.MapDemensionX = x ;
        this.MapDemensionY = y;

    }
}
enum Command
{
    Advance,
    Left,
    Right,
}
enum Direction
{
    South,
    North,
    West,
    East
}