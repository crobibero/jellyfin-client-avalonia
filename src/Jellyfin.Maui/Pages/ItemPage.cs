using CommunityToolkit.Maui.Converters;
using Jellyfin.Maui.ContentViews;
using Jellyfin.Maui.Converters;
using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Item page.
/// </summary>
public class ItemPage : BaseContentIdPage<ItemViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="ItemViewModel"/>.</param>
    public ItemPage(ItemViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = GridRowsColumns.Columns.Define(
                            (Column.Left, GridRowsColumns.Stars(3)),
                            (Column.Center, GridRowsColumns.Stars(6)),
                            (Column.Right, GridRowsColumns.Stars(3))),
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                new Label { Text = "Director" },
                                new Label { Text = "Runs" },
                                new Label { Text = "Ends" }
                            }
                            .Column(Column.Left),
                            new VerticalStackLayout
                            {
                                new Label()
                                            .Bind(Label.TextProperty, path: nameof(ViewModel.Title), mode: BindingMode.OneWay),
                                new Label()
                                    .Bind(Label.TextProperty, path: nameof(ViewModel.SubTitle), mode: BindingMode.OneWay),
                                new Label
                                {
                                    LineBreakMode = LineBreakMode.WordWrap,
                                    HorizontalTextAlignment = TextAlignment.Start
                                }
                                .Bind(Label.TextProperty, path: nameof(ViewModel.Description), mode: BindingMode.OneWay),

                            }
                            .Column(Column.Center),
                            new Image()
                                .Bind(Image.SourceProperty, path: nameof(ViewModel.Item), mode: BindingMode.OneWay, converter: new BaseItemDtoToImageSourceConverter(ImageType.Logo))
                                .Column(Column.Right)
                        }
                    },
                    new VerticalStackLayout
                    {
                        new Label { Text = Strings.NextUp },
                        new PosterCardView()
                            .Bind(BindingContextProperty, nameof(ViewModel.NextUpItem), mode: BindingMode.OneWay),
                    }
                    .Bind(VerticalStackLayout.IsVisibleProperty, nameof(ViewModel.NextUpItem), mode: BindingMode.OneWay, converter: new IsObjectNotNullConverter()),
                    new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label { Text = Strings.Seasons },
                            new CollectionView
                            {
                                ItemTemplate = TemplateHelper.CardTemplateSelector,
                                ItemsLayout = LinearItemsLayout.Horizontal,
                                ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
                                SelectionMode = SelectionMode.Single
                            }
                            .Bind(ItemsView.ItemsSourceProperty, mode: BindingMode.OneWay, path: nameof(ViewModel.Seasons))
                        }
                    }
                    .Bind(VerticalStackLayout.IsVisibleProperty, path: nameof(ViewModel.Seasons), mode: BindingMode.OneWay, converter: new IsListNotNullOrEmptyConverter()),
                    new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label { Text = Strings.Episodes },
                            new CollectionView
                            {
                                ItemTemplate = TemplateHelper.CardTemplateSelector,
                                ItemsLayout = LinearItemsLayout.Horizontal,
                                ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
                                SelectionMode = SelectionMode.Single
                            }
                            .Bind(ItemsView.ItemsSourceProperty, mode: BindingMode.OneWay, path: nameof(ViewModel.Episodes))
                        }
                    }
                    .Bind(VerticalStackLayout.IsVisibleProperty, path: nameof(ViewModel.Episodes), mode: BindingMode.OneWay, converter: new IsListNotNullOrEmptyConverter())
                }
            }
        };
    }

    private enum Column
    {
        Left = 0,
        Center = 1,
        Right = 2
    }
}
