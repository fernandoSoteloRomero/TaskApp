using Mapster;
using TaskApp.DTOs.Category;
using TaskApp.DTOs.Task;
using TaskApp.Models;

namespace TaskApp.Mapper;

public class MappingConfig
{
  public static void RegisterMappings()
  {
    var config = TypeAdapterConfig.GlobalSettings;


    // * Category -> CategoryDto
    config.NewConfig<Category, CategoryDto>();

    // * CategoryCreateDto -> Category
    config.NewConfig<CategoryCreateDto, Category>();

    // * CategoryUpdateDto -> Category
    config.NewConfig<CategoryUpdateDto, Category>();

    // * TaskItem -> TaskItemDto
    config.NewConfig<TaskItem, TaskItemDto>()
      .Map(dest => dest.CategoryName, src => src.Category.Name);

    // * TaskItemCreateDto -> TaskItem
    config.NewConfig<TaskItemCreateDto, TaskItem>()
      .Map(dest => dest.Status, _ => Status.Pending)
      .IgnoreNullValues(true);


    config.NewConfig<TaskItemUpdateDto, TaskItem>()
      .IgnoreNullValues(true);
  }
}
