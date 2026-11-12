// Управление каруселью фотографий на главной странице.
document.querySelectorAll('[data-carousel]').forEach((carousel) => {
    const slides = Array.from(carousel.querySelectorAll('.carousel-slide'));
    const indicators = Array.from(carousel.querySelectorAll('[data-carousel-to]'));
    const previousButton = carousel.querySelector('[data-carousel-prev]');
    const nextButton = carousel.querySelector('[data-carousel-next]');
    let activeIndex = 0;

    if (slides.length === 0) return;

    const showSlide = (index) => {
        activeIndex = (index + slides.length) % slides.length;
        slides.forEach((slide, slideIndex) => {
            const isActive = slideIndex === activeIndex;
            slide.classList.toggle('is-active', isActive);
            slide.setAttribute('aria-hidden', String(!isActive));
        });
        indicators.forEach((indicator, indicatorIndex) => {
            const isActive = indicatorIndex === activeIndex;
            indicator.classList.toggle('is-active', isActive);
            if (isActive) indicator.setAttribute('aria-current', 'true');
            else indicator.removeAttribute('aria-current');
        });
    };

    previousButton?.addEventListener('click', () => showSlide(activeIndex - 1));
    nextButton?.addEventListener('click', () => showSlide(activeIndex + 1));
    indicators.forEach((indicator) => {
        indicator.addEventListener('click', () => showSlide(Number(indicator.dataset.carouselTo)));
    });
    showSlide(0);
});

// Форма обратной связи пока является демонстрационной: данные никуда не отправляются.
const feedbackForm = document.getElementById('feedbackForm');
if (feedbackForm) {
    const notice = document.getElementById('feedbackNotice');
    const nameInput = document.getElementById('feedbackName');
    const emailInput = document.getElementById('feedbackEmail');

    const setError = (input, message) => {
        const error = feedbackForm.querySelector(`[data-error-for="${input.id}"]`);
        if (error) error.textContent = message;
        input.classList.toggle('is-invalid', Boolean(message));
        input.setAttribute('aria-invalid', String(Boolean(message)));
    };

    [nameInput, emailInput].forEach((input) => {
        input.addEventListener('input', () => setError(input, ''));
    });

    feedbackForm.addEventListener('submit', (event) => {
        event.preventDefault();
        notice.textContent = '';

        const name = nameInput.value.trim();
        const email = emailInput.value.trim();
        let isValid = true;

        if (!name) {
            setError(nameInput, 'Введите имя.');
            isValid = false;
        } else {
            setError(nameInput, '');
        }

        if (!email) {
            setError(emailInput, 'Введите email.');
            isValid = false;
        } else if (!emailInput.validity.valid) {
            setError(emailInput, 'Введите корректный email.');
            isValid = false;
        } else {
            setError(emailInput, '');
        }

        if (!isValid) return;
        notice.textContent = 'Форма пока не подключена к отправке сообщений.';
    });
}
