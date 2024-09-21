$('#returnModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var solutionId = button.data('id');
    var modal = $(this);
    modal.find('#returnSolutionId').val(solutionId);
});

$('#rejectModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var solutionId = button.data('id');
    var modal = $(this);
    modal.find('#rejectSolutionId').val(solutionId);
});