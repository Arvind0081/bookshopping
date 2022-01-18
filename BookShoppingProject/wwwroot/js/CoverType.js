var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/CoverType/GetAll"
        },
        "columns": [
            { "data": "name", "width": "80%", "className": "dt-center" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                      <a href="/Admin/CoverType/Upsert/${data}" class="btn btn-info"><i class="fas fa-edit"></i></a>
                      <a onclick=Delete("/Admin/CoverType/Delete/${data}") class="btn btn-danger"><i class="far fa-trash-alt"></i></a>

                            </div>`;
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
        dangerModel:true

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