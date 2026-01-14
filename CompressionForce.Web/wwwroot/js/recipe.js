
window.RecipeJs = (function () {
    function onIndexReady() {
        const $select = $('#recipeCodeSelect');
        const $recipeSection = $('#recipeSection');

        $select.on('change', function () {
            const code = $(this).val();
            if (!code) {
                $recipeSection.html('<div class="alert alert-info">No recipe selected.</div>');
                return;
            }
            $.get('/Recipe/GetRecipe', { code })
                .done(html => $recipeSection.html(html))
                .fail(xhr => $recipeSection.html(`<div class="alert alert-danger">${xhr.responseText || 'Error'}</div>`));
        });

        $('#btnAdd').on('click', function () {
            $('#newRecipeCode').val('');
            $('#codeAvailability').text('');
            $('#btnProceedAdd').prop('disabled', true);
            new bootstrap.Modal(document.getElementById('addCodeModal')).show();
        });

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

        $('#btnProceedAdd').on('click', function () {
            const code = $('#newRecipeCode').val().trim();
            if (code) window.location.href = `/Recipe/Add?code=${encodeURIComponent(code)}`;
        });

        $('#btnEdit').on('click', function () {
            const code = $('#recipeCodeSelect').val();
            if (!code) { alert('Select a recipe code first.'); return; }
            window.location.href = `/Recipe/Edit?code=${encodeURIComponent(code)}`;
        });

        $('#btnDelete').on('click', function () {
            const code = $('#recipeCodeSelect').val();
            if (!code) { alert('Select a recipe code first.'); return; }
            window.location.href = `/Recipe/Delete?code=${encodeURIComponent(code)}`;
        });

        $('#btnPrint').on('click', function () {
            const code = $('#recipeCodeSelect').val();
            if (!code) { alert('Select a recipe code first.'); return; }
            window.open(`/Recipe/Print?code=${encodeURIComponent(code)}`, '_blank');
        });
    }

    function attachClientValidation(formSelector) {
        const $form = $(formSelector);
        if (!$.validator || !$form.length) return;

        $.validator.addMethod("paramrange", function (value, element) {
            const $el = $(element);
            if (($el.data('param-type') || '').toLowerCase() !== 'numeric') return true;
            if (value === '' || value === null) return false;
            const v = parseFloat(value);
            const min = parseFloat($el.data('min') || '0');
            const max = parseFloat($el.data('max') || '100000');
            return !isNaN(v) && v >= min && v <= max;
        }, "Value out of allowed range.");

        $.validator.addMethod("enumrequired", function (value, element) {
            const $el = $(element);
            if (($el.data('param-type') || '').toLowerCase() !== 'enum') return true;
            return !!value && value.trim().length > 0;
        }, "Please select a value.");

        $form.find('.recipe-param').each(function () {
            const $el = $(this);
            const type = ($el.data('param-type') || '').toLowerCase();
            $el.rules('add', { required: $el.attr('required') !== undefined });

            if (type === 'numeric') $el.rules('add', { number: true, paramrange: true });
            if (type === 'text') $el.rules('add', { maxlength: parseInt($el.attr('maxlength') || '200', 10) });
            if (type === 'enum') $el.rules('add', { enumrequired: true });
        });

        $form.on('submit', function () { return $form.valid(); });
    }

    $(function () { if ($('#recipeCodeSelect').length) onIndexReady(); });

    return { attachClientValidation };
})();
