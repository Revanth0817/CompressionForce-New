
function updateRecipeCodeSelection(code) {
    const currentCodeEl = document.getElementById('currentRecipeCode');
    if (currentCodeEl) currentCodeEl.value = code;
    console.log('currentCodeEl:', currentCodeEl);
    console.log('code:', code);
    // Fetch partial HTML and replace the wrapper
    fetch('/Recipe/GetTheRecipe?code=' + encodeURIComponent(code), { method: 'GET' })
        .then(resp => {
            if (!resp.ok) throw new Error('Failed to load recipe partial');
            return resp.text();
        })
        .then(html => {
            const host = document.getElementById('recipeSection');
            if (host) host.innerHTML = html;

            // Close any open dropdown gracefully
            document.querySelectorAll('.dropdown.show .dropdown-btn')?.forEach(btn => {
                bootstrap.Dropdown.getOrCreateInstance(btn).hide();
            });
        })
        .catch(err => console.error('Error:', err));
}
function onIndexReady() {

    $('#btnCheckCode').on('click', function () {
        const code = $('#newRecipeCode').val().trim();
        if (!code) {
            $('#codeAvailability').text('Please enter a recipe code.');
            $('#btnProceedAdd').prop('disabled', true);
            return;
        }
        $.get('/Recipe/CheckCode', { code })
            .done(res => {
                $('#codeAvailability').text(res.message);
                $('#btnProceedAdd').prop('disabled', res.exists);
            })
            .fail(() => {
                $('#codeAvailability').text('Error checking code.');
                $('#btnProceedAdd').prop('disabled', true);
            });
    });

    $('#btnEdit').on('click', function () {
        const code = $('#currentRecipeCode').val();
        if (!code) { alert('Select a recipe code first.'); return; }
        window.location.href = `/Recipe/EditRecipe?code=${encodeURIComponent(code)}`;
    });

    $('#btnRemove').on('click', function () {
        const code = $('#currentRecipeCode').val();
        if (!code) { alert('Select a recipe code first.'); return; }
        window.location.href = `/Recipe/RemoveRecipe?code=${encodeURIComponent(code)}`;
    });

    $('#btnPrint').on('click', function () {
        const code = $('#currentRecipeCode').val();
        if (!code) { alert('Select a recipe code first.'); return; }
        window.open(`/Recipe/Print?code=${encodeURIComponent(code)}`, '_blank');
    });

    $('#btnConfirmAdd').on('click', function () {
        const codeValue = $('#recipeName').val().trim();
        const $errorDiv = $('#duplicateError');
        const $inputField = $('#recipeName');

        if (!codeValue) {
            alert('Please enter a Recipe Name/Code.');
            return;
        }

        fetch(`/Recipe/CheckCode?code=${encodeURIComponent(codeValue)}`)
            .then(response => response.json())
            .then(data => {
                if (data.exists) {
                    $errorDiv.show();
                    $inputField.addClass('is-invalid');
                } else {
                    $errorDiv.hide();
                    $inputField.removeClass('is-invalid');
                    window.location.href = `/Recipe/AddRecipe?code=${encodeURIComponent(codeValue)}`;
                }
            })
            .catch(err => {
                console.error('Error:', err);
                alert('An error occurred while checking the recipe code.');
            });
    });

    $('#recipeName').on('input', function () {
        $('#duplicateError').hide();
        $(this).removeClass('is-invalid');
    });


}

/*
function updateDropdownSelection(e, value) {
    e.preventDefault();  // Prevent the default behavior (e.g., page scroll, form submission)
    e.stopPropagation();

    // 1. Get the element that was actually clicked
    const clickedElement = e.currentTarget;

    // 2. Find the closest parent container that holds this specific dropdown
    // This works for both the "batchDropdown" divs and the Bootstrap "dropdown" divs
    const container = clickedElement.closest('.batchDropdown') || clickedElement.closest('.dropdown');

    if (container) {
        // 3. Update the visible text
        // Check if it's an input field (Tool Type/AWC) or a button (Force Feeder)
        const textDisplay = container.querySelector('.dropdownInput') || container.querySelector('.dropdown-toggle');

        if (textDisplay) {
            if (textDisplay.tagName === 'INPUT') {
                textDisplay.value = value;
            } else {
                // If it's a button, we keep the arrow icon/formatting by just changing text
                textDisplay.childNodes[0].textContent = value + ' ';
            }
        }



        // 4. Update any hidden inputs (for Form Submission)
        const hiddenInputs = container.querySelectorAll('input[type="hidden"]');
        hiddenInputs.forEach(input => {
            // Only update the input meant for the "Value"
            //if (input.name.includes('.Value') || input.getAttribute('recipe-id')) {
            if (input.name.includes('.Value')) {
                input.value = value;
            }
        });
    }
}
*/

/**
 * Updates the dropdown UI and hidden inputs
 * @param {Event} e - The click event object
 * @param {string} value - The value selected from the list
 */
    function updateDropdownSelection(e, value) {
    // Prevent the page from jumping/reloading
        e.preventDefault();
        console.log(e.currentTarget.firstChild.textContent);
        console.log(e.currentTarget.textContent);
    // 1. Get the element that was clicked (the <a> tag)
    const clickedElement = e.currentTarget;

    // 2. Find the container (supports both your custom and Bootstrap layouts)
    const container = clickedElement.closest('.batchDropdown') || clickedElement.closest('.dropdown');

    if (container) {
        // 3. Update the visible text/input
        const textDisplay = container.querySelector('.dropdownInput') || container.querySelector('.dropdown-toggle');

        if (textDisplay) {
            if (textDisplay.tagName === 'INPUT') {
                textDisplay.value = value;
            } else {
                // For buttons: Update text while preserving the toggle arrow if it exists
                textDisplay.firstChild.textContent = value;
            }
        }

        // 4. Update hidden inputs for form submission
        // This targets inputs with name like "Parameters[0].Value"
        const hiddenValueInput = container.querySelector('input[name$=".Value"]');
        if (hiddenValueInput) {
            hiddenValueInput.value = value;
        }
    }
}


$(function () { if ($('#currentRecipeCode').length) onIndexReady(); });
