using System.IO;
using HataBookingService.Data.DTOs;
using HataBookingService.Data.Models;
using HataBookingService.Data.Repositories;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace HataBookingService.Data.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<Booking> GetByIdAsync(Guid id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Booking booking)
        {
            await _bookingRepository.AddAsync(booking);
        }

        public async Task UpdateAsync(Booking booking)
        {
            await _bookingRepository.UpdateAsync(booking);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _bookingRepository.DeleteAsync(id);
        }

        public async Task<Booking> CreateBookingAsync(BookingDto bookingDto, int userId)
        {
            // Рассчитываем общую стоимость (предполагаем, что PropertyService внедрен и имеет метод GetPropertyPriceAsync)
            var totalPrice = (bookingDto.EndDate - bookingDto.StartDate).Days * await GetPropertyPriceAsync(bookingDto.PropertyId);

            var booking = new Booking
            {
                PropertyId = bookingDto.PropertyId,
                UserId = userId,
                StartDate = bookingDto.StartDate,
                EndDate = bookingDto.EndDate,
                TotalPrice = totalPrice,
                DateCreated = DateTime.UtcNow
            };

            // Генерируем PDF-договор
            var contractContent = GenerateContractContent(booking);
            var contractBytes = GeneratePdf(contractContent);

            // Сохраняем договор в виде Base64 строки
            booking.Contract = Convert.ToBase64String(contractBytes);

            await _bookingRepository.AddAsync(booking);
            return booking;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _bookingRepository.GetBookingsByUserIdAsync(userId);
        }

        private async Task<decimal> GetPropertyPriceAsync(Guid propertyId)
        {
            // Placeholder for actual property price fetching logic
            return 100; // Assume 100 currency units per day for example
        }

        private string GenerateContractContent(Booking booking)
        {
            return $@"
            Договор аренды (найма) жилого помещения

            Город __________

            «__»_____________________ года

            _____________________________________(Ф.И.О., реквизиты документа удостоверяющего личность), именуемый в дальнейшем «Наймодатель», действующий на основании _______________________________________, с одной стороны и ______________________________________ (Ф.И.О., реквизиты документа удостоверяющего личность), в дальнейшем именуемый «Наниматель», именуемые в дальнейшем совместно «Стороны», заключили настоящий договор о нижеследующем:

            1. Предмет договора

            1.1. Наймодатель обязуется за обусловленную Сторонами договора плату предоставить Нанимателю во временное владение и пользование жилое помещение (квартира/частный дом/комната), пригодное для постоянного проживания в нем и расположенное по адресу: ________________________________________, №_______, общей площадью ______кв. м, на____ этаже.

            Объект принадлежит Наймодателю на основании договора купли-продажи/аренды/мены/долевого участия в строительстве/свидетельства о праве на наследство и т. д. (нужное подчеркнуть): номер ______________, выданный _______________________________________ (орган, выдавший правоустанавливающие документа)_____________ года.

            1.2. По настоящему договору помимо Нанимателя в помещении будут постоянно проживать следующие граждане: _________________________________________________

            (Ф.И.О., реквизиты документа удостоверяющего личность).

            1.3. Настоящий договор вступает в силу с момента подписания и действует ____ месяцев, то есть до ____________ года.
            ";
        }

        private byte[] GeneratePdf(string content)
        {
            using (var stream = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Использование стандартного шрифта
                var font = new XFont("Verdana", 12, XFontStyle.Regular);

                // Разделим контент на строки
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Определим начальную позицию для текста
                double yPoint = 0;
                double lineHeight = font.GetHeight();

                foreach (var line in lines)
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(0, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    yPoint += lineHeight;
                }

                document.Save(stream, false);
                return stream.ToArray();
            }
        }
    }
    
}
