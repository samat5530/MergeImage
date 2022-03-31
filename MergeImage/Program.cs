using System.Drawing;

namespace MergeImage
{
    /// <summary>
    /// "Строка" в дереве изображений. Является "ячейкой"
    /// </summary>
    public class Row : Cell
    {
        public List<Cell> Items { get; set; } = new List<Cell>();

        public Row Add(Cell cell)
        {
            Items.Add(cell);
            return this;
        }

    }

    /// <summary>
    /// Базовая величина дерева - ячейка. Хранит экземпляр изображения
    /// </summary>
    public class Cell
    {
        public Image Image { get; set; }

        /// <summary>
        /// Возвращает true если ячейка является колонкой
        /// </summary>
        public bool IsColumn
        {
            get { return this.GetType() == typeof(Column); }
        }
        
        /// <summary>
        /// Возвращает false если ячейка является строкой
        /// </summary>
        public bool IsRow
        {
            get { return this.GetType() == typeof(Row); }
        }
    }

    /// <summary>
    /// "Колонка" в дереве изображений. Является "ячейкой"
    /// </summary>
    public class Column : Cell
    {
        public List<Cell> Items { get; set; } = new List<Cell>();

        public Column Add(Cell cell)
        {
            Items.Add(cell);
            return this;
        }
    }

    class Program
    {     

        /// <summary>
        /// Объединяет изображения в колонке в одно
        /// </summary>  
        public static Bitmap MergeImagesInColumn(params Image[] Images)
        {
            // Проверка на пустоту упакованного массива
            if (Images == null || Images.Length == 0)
                throw new ArgumentException("`Images` can't be empty!", "Images");

            // Получаем минимальную ширину, на которую позже будем ориентироваться
            // (Попутно проверяем изображения на null)
            int minWidth = Images.OrderBy(x => (x ?? throw new NullReferenceException()).Width).First().Width;

            // Получаем коллекцию отмасштабированных изображений
            IEnumerable<Image> resized = Images.Select(x => x.Width > minWidth ? ResizeImgByWidth(x, minWidth) : x);

            // Создаем Bitmap с минимальной шириной и суммарной высотой результирующих картинок
            Bitmap result = new Bitmap(minWidth, resized.Sum(x => x.Height));

            // Создаем Graphics из заданного изображения
            using (Graphics g = Graphics.FromImage(result))
            {
                int currentHeight = 0;
                foreach (Image img in resized)
                {
                    // Отрисовываем текущую картинку, увеличивая шаг на ее высоту
                    g.DrawImage(img, new Point(0, currentHeight));
                    currentHeight += img.Height;
                }
            }
            return result;
        }

        /// <summary>
        /// Объединяет изображения в строке в одно
        /// </summary>
        public static Bitmap MergeImagesInRow(params Image[] Images)
        {
            // Проверка на пустоту упакованного массива
            if (Images == null || Images.Length == 0)
                throw new ArgumentException("`Images` can't be empty!", "Images");

            // Получаем минимальную высоту, на которую позже будем ориентироваться
            // (Попутно проверяем изображения на null)
            int minHeight = Images.OrderBy(x => (x ?? throw new NullReferenceException()).Height).First().Height;

            // Получаем коллекцию отмасштабированных изображений
            //IEnumerable<Image> resized = Images.Select(x => x.Width > minWidth ? ResizeImgByWidth(x, minWidth) : x);

            IEnumerable<Image> resized = Images.Select(x => x.Height > minHeight ? ResizeImgByHeight(x, minHeight) : x);

            // Создаем Bitmap с минимальной высотой и суммарной шириной результирующих картинок

            //Bitmap result = new Bitmap(minHeight, resized.Sum(x => x.Height));
            Bitmap result = new Bitmap(resized.Sum(x => x.Width), minHeight);

            // Создаем Graphics из заданного изображения
            using (Graphics g = Graphics.FromImage(result))
            {
                int currentWidth = 0;
                foreach (Image img in resized)
                {
                    // Отрисовываем текущую картинку, увеличивая шаг на ее высоту
                    g.DrawImage(img, new Point(currentWidth, 0));
                    currentWidth += img.Width;
                }
            }
            return result;
        }

        /// <summary>
        /// Рисует раскадровку с заданной шириной 
        /// </summary>
        public static Bitmap drawStoryboard(Row row, int constraintWIdth)
        {
            // Отрисовывем корневую строку row
            row.Image = DrawRow(row);
            return ResizeImgByWidth(row.Image, constraintWIdth);
        }

        /// <summary>
        /// Рисует изображение в колонке
        /// </summary>
        public static Bitmap DrawColumn(Column column)
        {
            // Поиск строк в колонке column, и если colunm содержит строки, то отрисовываем строки.
            foreach(var item in column.Items)
            {
                if(item.IsRow)
                {
                    var rowInColumn = item as Row;
                    rowInColumn.Image = DrawRow(rowInColumn);
                }
            }

            //Отрисовка изображений в колонке column
            Image[] images = column.Items.Select(item => item.Image).ToArray();
            return MergeImagesInColumn(images);
        }

        /// <summary>
        /// Рисует изображение в строке
        /// </summary>
        public static Bitmap DrawRow(Row row)
        {
            // Поиск колонок в строке row, и если row содержит колонки, то отрисовываем колонки.
            foreach (var item in row.Items)
            {
                if (item.IsColumn)
                {
                    var column = item as Column;
                    column.Image = DrawColumn(column);
                }
            }

            // Отрисовка изображений в строке row
            Image[] images = row.Items.Select(item => item.Image).ToArray();
            return MergeImagesInRow(images);

        }

        /// <summary>
        /// Масштабирование изображения по заданной ширине
        /// </summary>
        private static Bitmap ResizeImgByWidth(Image Img, int NewWidth) => new Bitmap(Img, new Size(NewWidth, (int)(Img.Height * (NewWidth / (double)Img.Width))));

        /// <summary>
        /// Масштабирование изображения по заданной высоте
        /// </summary>
        private static Bitmap ResizeImgByHeight(Image img, int newHeight) => new Bitmap(img, new Size((int)(img.Width * (newHeight / (double)img.Height)), newHeight));

        public static void Main(string[] args)
        {

            Row r1 = new Row();
            Row r2 = new Row();
            Row r3 = new Row();
            Column c1 = new Column();
            Column c2 = new Column();
            
            Cell img1 = new Cell() { Image = Image.FromFile("test0.jpg") };
            Cell img2 = new Cell() { Image = Image.FromFile("test1.jpg") };
            Cell img3 = new Cell() { Image = Image.FromFile("test2.jpg") };


            r1.Add(img1).Add(c1.Add(r2.Add(img1).Add(img2)).Add(img3)).Add(img2);

            var result = drawStoryboard(r1, 1000);
            result.Save("result.jpeg");
           
        }
    }
}