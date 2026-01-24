function saveBatch() {
    const recipeCode = $('#selectedItem').val();

    $.post('/Batch/AddBatch', {
        recipeCode: recipeCode,
        batchCode: $('#batchCode').val(),
        batchQty: $('#batchQty').val(),
        tabletQty: $('#tabletQty').val()
    }).done(() => {
        location.reload();
    });
}


function saveEditedBatch() {
    const recipeCode = $('#selectedItem').val();

    $.post('/Batch/EditBatch', {
        batchCode: $('#editBatchCode').val(),
        batchQty: $('#editBatchQty').val()
    }).done(() => {
        location.reload();
    });
}


function deactivateBatch() {
    const recipeCode = $('#selectedItem').val();

    $.post('/Batch/DeactivateBatch', {
        batchCode: $('#deactivateBatchCode').val()
    }).done(() => {
        location.reload();
    });
}

function refreshBatchHeader(recipeCode) {
    if (!recipeCode) return;

    $.get('/Batch/GetBatchHeader', { recipeCode })
        .done(function (html) {
            $('#batchHeaderContainer').html(html);
        });
}


$(document).ready(function () {

    // Recipe selection (custom dropdown)
    $(document).on('click', '.RecipeCodeDropdown .dropdown-item', function (e) {
        e.preventDefault();

        const recipeCode = $(this).data('recipecode');
        $('#selectedItem').val(recipeCode);

        $.get('/Batch/GetBatchesByRecipe', { recipeCode })
            .done(function (batches) {

                refreshBatchHeader(recipeCode);
                populateBatchDropdown([]);

                if (!batches || batches.length === 0) {

                    
                    $('#batchSummaryContainer').html(`
                    <div class="p-lg-4 p-md-4 p-sm-3 p-3 bord-D2D2D2 bord-rad-4px boxshadow">
                        <div class="text-muted">No batches</div>
                    </div>
                `);

                    loadRecipeParameters(recipeCode);
                    return;
                }

                populateBatchDropdown(batches);

                const firstBatchCode = batches[0].batchCode;
                $('#batchSelectedItem').val(firstBatchCode);

                loadBatchSummaryAndParameters(firstBatchCode);
            });
    });

});

$(document).on('click', '#batchDropdownMenu .dropdown-item', function (e) {
    e.preventDefault();

    const batchCode = $(this).data('batchcode');
    $('#batchSelectedItem').val(batchCode);

    loadBatchSummaryAndParameters(batchCode);
});

$(document).ready(function () {

    const recipeCode = $('#selectedItem').val();
    const selectedBatchCode = $('#batchSelectedItem').val();

    if (!recipeCode) return;

    $.get('/Batch/GetBatchesByRecipe', { recipeCode })
        .done(function (batches) {

            populateBatchDropdown(batches);

            if (selectedBatchCode) {
                $('#batchSelectedItem').val(selectedBatchCode);
            }
        });
});

$('#editBatchModal').on('show.bs.modal', function () {

    const recipeCode = $('#selectedItem').val();

    $.get('/Batch/GetBatchesByRecipe', { recipeCode })
        .done(function (batches) {

            const ddl = $('#editBatchCode');
            ddl.empty();
            ddl.append(`<option value="">Select Batch</option>`);

            batches
                .filter(b => b.batchStatus !== 'Deactivated')
                .forEach(b => {
                    ddl.append(`<option value="${b.batchCode}">${b.batchCode}</option>`);
                });
        });
});

$('#deactivateBatchModal').on('show.bs.modal', function () {

    const recipeCode = $('#selectedItem').val();

    $.get('/Batch/GetBatchesByRecipe', { recipeCode })
        .done(function (batches) {

            const ddl = $('#deactivateBatchCode');
            ddl.empty();
            ddl.append(`<option value="">Select Batch</option>`);

            batches
                .filter(b => b.batchStatus !== 'Deactivated')
                .forEach(b => {
                    ddl.append(`<option value="${b.batchCode}">${b.batchCode}</option>`);
                });
        });
});

$('#editBatchCode').on('change', function () {
    const batchCode = $(this).val();
    if (!batchCode) return;

    $.get('/Batch/GetBatchDetails', { batchCode })
        .done(function (data) {
            $('#editBatchQty').val(data.batchQty);
        });
});

function loadBatchSummaryAndParameters(batchCode) {
    if (!batchCode) return;

    $.get('/Batch/GetBatchSummaryAndParameters', { batchCode })
        .done(function (data) {
            $('#batchSummaryContainer').html(data.summaryHtml);
            $('#recipeParametersContainer').html(data.parametersHtml);

            // REPOPULATE batch dropdown after DOM replacement
            const recipeCode = $('#selectedItem').val();
            refreshBatchHeader(recipeCode);
            $.get('/Batch/GetBatchesByRecipe', { recipeCode })
                .done(function (batches) {
                    populateBatchDropdown(batches);
                    $('#batchSelectedItem').val(batchCode);
                });
        });
}
function loadRecipeParameters(recipeCode) {
    if (!recipeCode) return;

    $.get('/Batch/GetRecipeParametersHtml', { recipeCode })
        .done(function (html) {

            $('#recipeParametersContainer').html(html);

            $('#batchSummaryContainer').html(`
                <div class="p-lg-4 p-md-4 p-sm-3 p-3 bord-D2D2D2 bord-rad-4px boxshadow">
                    <div class="text-muted">No batches</div>
                </div>
            `);
        });
}
function populateBatchDropdown(batches) {
    const menu = $('#batchDropdownMenu');
    if (menu.length === 0) return;

    menu.empty();

    if (!batches || batches.length === 0) return;

    batches.forEach(b => {
        menu.append(`
            <li>
                <a class="dropdown-item" href="#" data-batchcode="${b.batchCode}">
                    ${b.batchCode}
                </a>
            </li>
        `);
    });
}

//Helpers
function isNotEmpty(val) {
    return val !== undefined && val !== null && val.toString().trim() !== '';
}
function isPositiveNumber(val) {
    return !isNaN(val) && Number(val) > 0;
}


//Validations and Validation Bindings
function validateAddBatchModal() {
    const batchCode = $('#batchCode').val();
    const batchQty = $('#batchQty').val();
    const tabletQty = $('#tabletQty').val();

    const isValid =
        isNotEmpty(batchCode) &&
        isPositiveNumber(batchQty) &&
        isNotEmpty(tabletQty);

    $('#addBatchModal button[onclick="saveBatch()"]')
        .prop('disabled', !isValid);
}

$('#addBatchModal').on('shown.bs.modal', function () {
    validateAddBatchModal();
});

$('#batchCode, #batchQty, #tabletQty').on('input change', function () {
    validateAddBatchModal();
});

function validateEditBatchModal() {
    const batchCode = $('#editBatchCode').val();
    const batchQty = $('#editBatchQty').val();

    const isValid =
        isNotEmpty(batchCode) &&
        isPositiveNumber(batchQty);

    $('#editBatchModal button[onclick="saveEditedBatch()"]')
        .prop('disabled', !isValid);
}

$('#editBatchModal').on('show.bs.modal', function () {

    const recipeCode = $('#selectedItem').val();
    if (!recipeCode) return;

    $.get('/Batch/GetActiveBatchesByRecipe', { recipeCode })
        .done(function (batches) {

            const ddl = $('#editBatchCode');
            ddl.empty();
            ddl.append(`<option value="">Select Batch</option>`);

            if (!batches || batches.length === 0) { return; }

            batches.forEach(b => {
                ddl.append(`<option value="${b.batchCode}">${b.batchCode}</option>`);
            });
        });
});

$('#editBatchCode, #editBatchQty').on('input change', function () {
    validateEditBatchModal();
});

function validateDeactivateBatchModal() {
    const batchCode = $('#deactivateBatchCode').val();

    const isValid = isNotEmpty(batchCode);

    $('#deactivateBatchModal button[onclick="deactivateBatch()"]')
        .prop('disabled', !isValid);
}

$('#deactivateBatchModal').on('show.bs.modal', function () {

    const recipeCode = $('#selectedItem').val();
    if (!recipeCode) return;

    $.get('/Batch/GetActiveBatchesByRecipe', { recipeCode })
        .done(function (batches) {

            const ddl = $('#deactivateBatchCode');
            ddl.empty();
            ddl.append(`<option value="">Select Batch</option>`);

            if (!batches || batches.length === 0) { return; }

            batches.forEach(b => {
                ddl.append(`<option value="${b.batchCode}">${b.batchCode}</option>`);
            });
        });
});

$('#deactivateBatchCode').on('change', function () {
    validateDeactivateBatchModal();
});