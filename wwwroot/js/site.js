// Sidebar Toggle
$(document).ready(function () {
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Close sidebar on mobile when clicking outside
    $(document).on('click', function (e) {
        if ($(window).width() <= 768) {
            if (!$(e.target).closest('#sidebar, #sidebarCollapse').length) {
                $('#sidebar').removeClass('active');
                $('#content').removeClass('active');
            }
        }
    });

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);

    // Form validation
    $('form').on('submit', function () {
        var isValid = true;
        $(this).find('[required]').each(function () {
            if (!$(this).val()) {
                $(this).addClass('is-invalid');
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });
        return isValid;
    });

    // Remove validation classes on input
    $('input, select, textarea').on('input change', function () {
        $(this).removeClass('is-invalid');
    });
});

// Utility functions
function showLoading(element) {
    $(element).html('<div class="spinner mx-auto"></div>');
}

function hideLoading(element, content) {
    $(element).html(content);
}

function showAlert(message, type = 'success') {
    var alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    $('.container-fluid').prepend(alertHtml);
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

function formatDate(date) {
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

// AJAX helper functions
function ajaxGet(url, successCallback, errorCallback) {
    $.ajax({
        url: url,
        type: 'GET',
        success: successCallback,
        error: errorCallback || function (xhr, status, error) {
            showAlert('An error occurred while loading data.', 'danger');
        }
    });
}

function ajaxPost(url, data, successCallback, errorCallback) {
    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: successCallback,
        error: errorCallback || function (xhr, status, error) {
            showAlert('An error occurred while saving data.', 'danger');
        }
    });
}

// Modal helper functions
function openModal(modalId) {
    $(modalId).modal('show');
}

function closeModal(modalId) {
    $(modalId).modal('hide');
}

// Table helper functions
function refreshTable(tableId, url) {
    showLoading(tableId);
    ajaxGet(url, function (data) {
        $(tableId).html(data);
    });
}

// Form helper functions
function resetForm(formId) {
    $(formId)[0].reset();
    $(formId + ' .is-invalid').removeClass('is-invalid');
}

function submitForm(formId, successCallback) {
    var form = $(formId);
    if (form[0].checkValidity()) {
        var formData = new FormData(form[0]);
        ajaxPost(form.attr('action'), formData, successCallback);
    } else {
        form[0].reportValidity();
    }
}

// Chart helper functions (if using Chart.js)
function createChart(canvasId, type, data, options) {
    var ctx = document.getElementById(canvasId).getContext('2d');
    return new Chart(ctx, {
        type: type,
        data: data,
        options: options
    });
}

// Notification helper functions
function updateNotificationCount(count) {
    $('.badge').text(count);
}

function markNotificationAsRead(notificationId) {
    ajaxPost('/Notification/MarkAsRead', { id: notificationId }, function () {
        // Update UI
    });
}

// Search helper functions
function debounce(func, wait) {
    var timeout;
    return function executedFunction(...args) {
        var later = function () {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

var debouncedSearch = debounce(function (searchTerm) {
    // Perform search
    ajaxGet('/Search?term=' + encodeURIComponent(searchTerm), function (data) {
        // Update search results
    });
}, 300);

// Initialize search functionality
$(document).on('input', '.search-input', function () {
    var searchTerm = $(this).val();
    if (searchTerm.length >= 2) {
        debouncedSearch(searchTerm);
    }
});

// Export functions
function exportToPdf(elementId, filename) {
    html2pdf().from(document.getElementById(elementId)).save(filename);
}

function exportToExcel(tableId, filename) {
    var table = document.getElementById(tableId);
    var wb = XLSX.utils.table_to_book(table, { sheet: "Sheet1" });
    XLSX.writeFile(wb, filename);
}

// Print functions
function printElement(elementId) {
    var printContents = document.getElementById(elementId).innerHTML;
    var originalContents = document.body.innerHTML;
    document.body.innerHTML = printContents;
    window.print();
    document.body.innerHTML = originalContents;
}

// Date picker initialization
$(document).ready(function () {
    if ($.fn.datepicker) {
        $('.datepicker').datepicker({
            format: 'yyyy-mm-dd',
            autoclose: true,
            todayHighlight: true
        });
    }
});

// Select2 initialization (if using Select2)
$(document).ready(function () {
    if ($.fn.select2) {
        $('.select2').select2({
            theme: 'bootstrap-5'
        });
    }
});

// DataTables initialization (if using DataTables)
$(document).ready(function () {
    if ($.fn.DataTable) {
        $('.datatable').DataTable({
            responsive: true,
            language: {
                search: "Search:",
                lengthMenu: "Show _MENU_ entries per page",
                info: "Showing _START_ to _END_ of _TOTAL_ entries",
                infoEmpty: "Showing 0 to 0 of 0 entries",
                infoFiltered: "(filtered from _MAX_ total entries)",
                paginate: {
                    first: "First",
                    last: "Last",
                    next: "Next",
                    previous: "Previous"
                }
            }
        });
    }
});
