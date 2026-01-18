function saveBatch() {
    $.post('/Batch/AddBatch', {
        recipeCode: $('#addRecipeCode').val(),
        batchCode: $('#addBatchCode').val(),
        batchQty: $('#addBatchQty').val(),
        tabletQty: $('#addTabletQty').val()
    }).done(() => location.reload());
}

function updateBatch() {
    $.post('/Batch/EditBatch', {
        batchCode: $('#editBatchCode').val(),
        batchQty: $('#editBatchQty').val()
    }).done(() => location.reload());
}

function deactivateBatch() {
    $.post('/Batch/DeactivateBatch', {
        batchCode: $('#deactivateBatchCode').val()
    }).done(() => location.reload());
}
