<?php $this->load->view('common/header'); ?>
<script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=true"></script>
<script type="text/javascript" src="<?php echo base_url(); ?>html/js/gmaps.js"></script>
<script type="text/javascript">
    var map;
    $(document).ready(function(){
        map = new GMaps({
            div: '#map_canvas',
            lat: $("#y").text(),
            lng: $("#x").text(),
            zoom:16
        });
        map.addMarker({
            lat: $("#y").text(),
            lng: $("#x").text(),
            title: $("#box_id").text()
        });
    });

</script>
<style>
#map_canvas {
    width: 100%;
    height:500px;
}
</style>
<div class="container-fluid">
    <h2>端末詳細</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">端末情報</div>
        </div>
        <div class="panel-body">
            <table class="table table-bordered">
                <tr>
                    <th>ID</th>
                    <td id='box_id'><?php echo $box->id; ?></td>
                </tr>
                <tr>
                    <th>端末登録日</th>
                    <td><?php echo $box->entry_date; ?></td>
                </tr>
                <tr>
                    <th>端末破棄日</th>
                    <td><?php echo $box->delete_date; ?></td>
                </tr>
                <tr>
                    <th>経度</th>
                    <td id="x"><?php echo $box->x; ?></td>
                </tr>
                <tr>
                    <th>緯度</th>
                    <td id="y"><?php echo $box->y; ?></td>
                </tr>
                <tr>
                    <th>設置住所</th>
                    <td><?php echo $box->address; ?></td>
                </tr>
                <tr>
                    <th>マップ</th>
                    <td><div id="map_canvas"></div></td>
                </tr>
            </table>
        </div>
        <div class="text-center" style="margin-bottom:10px;">
            <button type="submit" class="btn btn-info btn-lg">
                <a onclick="location.href='<?php echo base_url() ?>index.php/box/index'"><i class="glyphicon glyphicon-chevron-left" onclick="search_user"></i>戻る</a>
            </button>
        </div>
    </div>
</div>
<script type="text/javascript" src="https://www.google.com/jsapi"></script>

<?php $this->load->view('common/footer'); ?>
