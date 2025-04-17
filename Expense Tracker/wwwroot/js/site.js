// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Enhanced UI interactions for Expense Tracker

document.addEventListener('DOMContentLoaded', function() {
    // Enable tooltips everywhere
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Add hover effects to cards
    const cards = document.querySelectorAll('.card, .stats-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-6px)';
            this.style.boxShadow = '0 12px 32px rgba(0, 0, 0, 0.5)';
            this.style.transition = 'all 0.3s ease';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = '';
            this.style.boxShadow = '';
        });
    });

    // Add smooth scrolling to all links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetId = this.getAttribute('href');
            if (targetId === '#') return;
            
            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                window.scrollTo({
                    top: targetElement.offsetTop - 80,
                    behavior: 'smooth'
                });
            }
        });
    });

    // Add number animation effect to any element with data-value attribute
    const numberElements = document.querySelectorAll('[data-value]');
    numberElements.forEach(element => {
        animateNumber(element);
    });

    // Function to animate numbers
    function animateNumber(element) {
        const value = parseFloat(element.getAttribute('data-value'));
        const duration = 1000; // Animation duration in milliseconds
        const frameDuration = 16; // Duration of a single frame in milliseconds (60fps)
        const totalFrames = duration / frameDuration;
        
        // Format options
        const isCurrency = element.textContent.includes('$') || 
                           element.textContent.includes('€') || 
                           element.textContent.includes('лв');
        
        let currentFrame = 0;
        const initialValue = 0;
        const increment = (value - initialValue) / totalFrames;
        
        const formatter = new Intl.NumberFormat('en-US', {
            style: isCurrency ? 'currency' : 'decimal',
            currency: 'USD',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
        
        const animate = () => {
            currentFrame++;
            const currentValue = initialValue + (increment * currentFrame);
            
            if (isCurrency) {
                // Keep the original currency symbol
                const originalSymbol = element.textContent.trim()[0];
                element.textContent = formatter.format(currentValue).replace('$', originalSymbol);
            } else {
                element.textContent = formatter.format(currentValue);
            }
            
            if (currentFrame < totalFrames) {
                requestAnimationFrame(animate);
            }
        };
        
        animate();
    }

    // Add animated background
    const body = document.body;
    if (body) {
        // Create the animated gradient
        createAnimatedBackground();
    }

    function createAnimatedBackground() {
        const gradientContainer = document.createElement('div');
        gradientContainer.classList.add('animated-gradient');
        gradientContainer.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            z-index: -1;
            background: linear-gradient(135deg, #0f0c29, #302b63, #24243e);
            background-size: 400% 400%;
            animation: gradientAnimation 15s ease infinite;
        `;
        
        // Add @keyframes rule for gradient animation
        const style = document.createElement('style');
        style.textContent = `
            @keyframes gradientAnimation {
                0% { background-position: 0% 50%; }
                50% { background-position: 100% 50%; }
                100% { background-position: 0% 50%; }
            }
        `;
        
        document.head.appendChild(style);
        document.body.prepend(gradientContainer);
    }
    
    // Enhanced error state for forms
    const formControls = document.querySelectorAll('.form-control, .form-select');
    formControls.forEach(control => {
        control.addEventListener('invalid', function() {
            this.classList.add('is-invalid');
            
            // Add shake animation
            this.style.animation = 'shake 0.5s';
            this.addEventListener('animationend', () => {
                this.style.animation = '';
            });
        });
        
        control.addEventListener('input', function() {
            this.classList.remove('is-invalid');
        });
    });
    
    // Add shake animation keyframes
    const shakeStyle = document.createElement('style');
    shakeStyle.textContent = `
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
            20%, 40%, 60%, 80% { transform: translateX(5px); }
        }
    `;
    document.head.appendChild(shakeStyle);

    // Set the selected values in dropdowns based on URL parameters
    function setSelectedValues() {
        const urlParams = new URLSearchParams(window.location.search);
        
        // Set transaction type dropdown
        const typeParam = urlParams.get('type');
        if (typeParam && document.getElementById('transactionType')) {
            document.getElementById('transactionType').value = typeParam;
        }
        
        // Set currency dropdown
        const currencyParam = urlParams.get('displayCurrency');
        if (currencyParam && document.getElementById('displayCurrency')) {
            document.getElementById('displayCurrency').value = currencyParam;
        }
        
        // Set active date range pill
        if (document.querySelectorAll('.date-range-pill').length > 0) {
            const startDate = document.getElementById('startDate').value;
            const endDate = document.getElementById('endDate').value;
            
            // Check if date range matches a predefined range
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            const startDateObj = new Date(startDate);
            const endDateObj = new Date(endDate);
            
            // Calculate days difference
            const diffTime = Math.abs(today - startDateObj);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
            
            // Check if end date is today and find appropriate pill
            if (formatDate(today) === endDate) {
                if (diffDays === 7) {
                    setActiveDatePill('7');
                } else if (diffDays === 30) {
                    setActiveDatePill('30');
                } else if (diffDays === 90) {
                    setActiveDatePill('90');
                } else {
                    setActiveDatePill('custom');
                }
            } else {
                setActiveDatePill('custom');
            }
        }
    }
    
    // Helper function to set active date pill
    function setActiveDatePill(range) {
        document.querySelectorAll('.date-range-pill').forEach(pill => {
            pill.classList.remove('active');
            if (pill.getAttribute('data-range') === range) {
                pill.classList.add('active');
            }
        });
    }
    
    // Call the function to set selected values
    setSelectedValues();

    // Make dropdown options readable
    const formatDropdowns = () => {
        const selects = document.querySelectorAll('select.form-select');
        selects.forEach(select => {
            // Ensure the select has the right styling
            select.style.color = 'white';
            select.style.backgroundColor = 'rgba(255, 255, 255, 0.05)';
            
            // Create a style element for dropdown options if it doesn't exist
            if (!document.getElementById('dropdown-styles')) {
                const style = document.createElement('style');
                style.id = 'dropdown-styles';
                style.textContent = `
                    select.form-select option {
                        background-color: #302b63;
                        color: white;
                        padding: 10px;
                    }
                `;
                document.head.appendChild(style);
            }
        });
    };
    
    // Call the function to format dropdowns
    formatDropdowns();
});
