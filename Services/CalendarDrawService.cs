using Bronya.Entities;
using Bronya.Enums;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using Bronya.Xtensions;

namespace Bronya.Services
{
    public class CalendarDrawService
    {
        public MemoryStream Draw(Table table)
        {
            const int workHourStringSize = 25;
            const int bookShortStringSize = 12;
            const int bookNormalStringSize = 75;

            BookService bookService = new BookService(null);
            var books = bookService.GetCurrentBooks(table).ToArray();
            Bitmap bmp = new Bitmap(1000, 1500);

            var smena = bookService.Smena;
            var smenaLength = smena.Schedule.Length;
            var oneHourHeight = bmp.Height / smenaLength.TotalHours;
            var hourSpace = 50;
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                for (var i = smena.SmenaStart; i < smena.SmenaEnd; i = i.AddHours(1))
                {
                    var hour = i.Date.Add(new TimeSpan(i.Hour, 0, 0));
                    if (hour != i)
                        continue;
                    int y = (int)(hour.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                    g.DrawString($"{i.Hour:00}", new Font("Tahoma", workHourStringSize), Brushes.Blue, new Point(3, y));
                    g.DrawLine(new Pen(Color.DarkBlue, 2), 0, y, bmp.Width, y);
                }
                foreach (var book in books.OrderBy(x => x.ActualBookStartTime))
                {
                    var bookStringSize = book.FactBookLength.TotalHours < 1 ? bookShortStringSize : bookNormalStringSize;
                    if (book.GetStatus() == BookStatus.Booked)
                    {
                        int yBook = (int)(book.ActualBookStartTime.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                        int hBook = (int)(book.BookLength.TotalHours * oneHourHeight);
                        g.FillRectangle(Brushes.LightGray,
                            new Rectangle(hourSpace, yBook, bmp.Width, hBook));

                        g.DrawString($"{book.GetTrueStartBook():HH:mm}-{book.GetTrueEndBook():HH:mm}",
                            new Font("Tahoma", bookStringSize),
                            Brushes.Black,
                            new Point(hourSpace + 5, yBook + 5));
                    }
                    else if (book.GetStatus() == BookStatus.Opened)
                    {
                        int yBook = (int)(book.TableAllowedStarted.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                        int hBook = (int)(book.BookLength.TotalHours * oneHourHeight);

                        g.FillRectangle(Brushes.Purple,
                            new Rectangle(hourSpace, yBook, bmp.Width, hBook));
                        g.DrawString($"{book.TableAllowedStarted:HH:mm}-{book.TableAllowedStarted.Add(book.BookLength):HH:mm}",
                            new Font("Tahoma", bookStringSize),
                            Brushes.White,
                            new Point(hourSpace + 5, yBook + 5));

                        int bufferLeft = (int)(book.BookEndTime.Add(smena.Schedule.Buffer).Subtract(book.TableAllowedStarted.Add(book.BookLength)).TotalHours * oneHourHeight);

                        g.FillRectangle(Brushes.Gray,
                            new Rectangle(hourSpace, yBook+hBook, bmp.Width, bufferLeft));
                    }
                    else if (book.GetStatus() == BookStatus.Closed)
                    {
                        int yBook = (int)(book.TableAllowedStarted.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                        int hBook = (int)(book.TableClosed.Subtract(book.TableAllowedStarted).TotalHours * oneHourHeight);
                        g.FillRectangle(Brushes.DarkBlue,
                            new Rectangle(hourSpace, yBook, bmp.Width, hBook));

                        g.DrawString($"{book.TableAllowedStarted:HH:mm}-{book.TableClosed:HH:mm}",
                            new Font("Tahoma", bookStringSize),
                            Brushes.White,
                            new Point(hourSpace + 5, yBook + 5));
                    }
                    else if (book.GetStatus() == BookStatus.Canceled)
                    {
                        continue;
                    }
                }

                int yNow = (int)(new TimeService().GetNow().Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                g.DrawLine(new Pen(Color.Red, 3), 0, yNow, bmp.Width, yNow);
            }

            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, ImageFormat.Jpeg);
            bmp.Save("Images\\image2.jpeg", ImageFormat.Jpeg);
            bmp.Dispose();
            stream.Position = 0;
            return stream;
        }

        public MemoryStream DrawFull()
        {
            const int workHourStringSize = 25;
            const int bookShortStringSize = 25;

            BookService bookService = new BookService(null);
            var tables = bookService.TableDS.GetAll().OrderBy(x => x.Number).ToArray();
            var allBooks = bookService.GetCurrentBooks().ToArray();
            Bitmap bmp = new Bitmap(1000, 1500);

            var smena = bookService.Smena;
            var smenaLength = smena.Schedule.Length;
            var oneHourHeight = bmp.Height / smenaLength.TotalHours;
            var hourSpace = 50;
            var tableColW = (bmp.Width - hourSpace) / tables.Count();

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                for (var i = smena.SmenaStart; i < smena.SmenaEnd; i = i.AddHours(1))
                {
                    var hour = i.Date.Add(new TimeSpan(i.Hour, 0, 0));
                    if (hour != i)
                        continue;
                    int y = (int)(hour.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                    g.DrawString($"{i.Hour:00}", new Font("Tahoma", workHourStringSize), Brushes.Blue, new Point(5, y));
                    g.DrawLine(new Pen(Color.DarkBlue, 3), 0, y, bmp.Width, y);
                }

                for (int t = 0; t < tables.Count(); t++)
                {
                    var shiftX = tableColW * t;
                    var table = tables[t];
                    var books = allBooks.Where(x => x.Table == table);
                    foreach (var book in books.OrderBy(x => x.ActualBookStartTime))
                    {
                        var bookStringSize = bookShortStringSize;
                        if (book.GetStatus() == BookStatus.Booked)
                        {
                            int yBook = (int)(book.ActualBookStartTime.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                            int hBook = (int)(book.BookLength.TotalHours * oneHourHeight);
                            g.FillRectangle(Brushes.LightGray,
                                new Rectangle(hourSpace + shiftX, yBook, tableColW, hBook));

                            g.DrawString($"{table.Name}\r\n{book.GetTrueStartBook():HH:mm}\r\n{book.GetTrueEndBook():HH:mm}",
                                new Font("Tahoma", bookStringSize),
                                Brushes.Black,
                                new Point(hourSpace + shiftX + 5, yBook + 5));
                        }
                        else if (book.GetStatus() == BookStatus.Opened)
                        {
                            int yBook = (int)(book.TableAllowedStarted.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                            int hBook = (int)(book.BookLength.TotalHours * oneHourHeight);
                            g.FillRectangle(Brushes.Purple,
                                new Rectangle(hourSpace + shiftX, yBook, tableColW, hBook));

                            g.DrawString($"{table.Name}\r\n{book.TableAllowedStarted:HH:mm}\r\n{book.GetTrueEndBook():HH:mm}",
                                new Font("Tahoma", bookStringSize),
                                Brushes.White,
                                new Point(hourSpace + shiftX + 5, yBook + 5));
                        }
                        else if (book.GetStatus() == BookStatus.Closed)
                        {
                            int yBook = (int)(book.TableAllowedStarted.Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                            int hBook = (int)(book.TableClosed.Subtract(book.TableStarted).TotalHours * oneHourHeight);
                            g.FillRectangle(Brushes.DarkBlue,
                                new Rectangle(hourSpace + shiftX, yBook, tableColW, hBook));

                            g.DrawString($"{table.Name}\r\n{book.TableAllowedStarted:HH:mm}\r\n{book.GetTrueEndBook():HH:mm}",
                                new Font("Tahoma", bookStringSize),
                                Brushes.White,
                                new Point(hourSpace + shiftX + 5, yBook + 5));
                        }
                        else if (book.GetStatus() == BookStatus.Canceled)
                        {
                            continue;
                        }
                    }
                }

                int yNow = (int)(new TimeService().GetNow().Round(TimeSpan.FromMinutes(10)).Subtract(smena.SmenaStart).TotalHours * oneHourHeight);
                g.DrawLine(new Pen(Color.Red, 3), 0, yNow, bmp.Width, yNow);
            }

            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, ImageFormat.Jpeg);
            //bmp.Save("C:\\Users\\marse\\source\\repos\\Bronya\\Images\\image2.jpeg", ImageFormat.Jpeg);
            bmp.Dispose();
            stream.Position = 0;
            return stream;
        }
    }
}
