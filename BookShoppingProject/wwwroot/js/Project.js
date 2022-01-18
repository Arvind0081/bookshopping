var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%", "className": "dt-center" },
            { "data": "discription", "width": "15%", "className": "dt-center" },
            { "data": "author", "width": "15%", "className": "dt-center" },
            { "data": "isbn", "width": "15%", "className": "dt-center" },
            { "data": "price", "width": "15%", "className": "dt-center" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                          <a class="btn btn-primary" href="/Admin/Product/Upsert/${data}"><i class="far fa-edit"></i></a>
                          <a class="btn btn-danger" onclick=Delete("/Admin/Product/Delete/${data}")><i class="far fa-trash-alt"></i></a>
                             </div>  `;
                }

            }

        ]
    })
}
function Delete(url) {

    swal({
        title: "Want to delete record?",
        text: "Deleted record can not be retrieve",
        icon: "warning",
        buttons: true,
        dangerModel: true

    }).then((willdelete) => {
        if (willdelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}