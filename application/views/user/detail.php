<?php echo $this->load->view('common/header') ?>
<div class="container-fluid">
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">ユーザ情報</div>
        </div>
        <div class="panel-body">
            <table class="table table-bordered">
                <?php
                foreach ($user as $val) {
                    ?>
                    <tr>
                        <th>ニックネーム</th>
                        <td><?php echo $val->nickname ?></td>
                    </tr>
                    <tr>
                        <th>登録日</th>
                        <td><?php echo $val->entry_date ?></td>
                    </tr>
                    <tr>
                        <th>更新日</th>
                        <td><?php echo $val->update_date ?></td>
                    </tr>
                    <rt>
                    <th>生年月日</th>
                    <td><?php echo $val->birthday ?></td>
                    </tr>
                    <?php
                }
                ?>
            </table>
        </div>
    </div>
    <table class="table table-hover">
        <tbody>
            <tr class="success">
                <th class="col-xs-1 col-sm-1 col-md-1">端末ID</th>
                <th class="col-xs-1 col-sm-1 col-md-1">曲ID</th>
                <th class="col-xs-2 col-sm-2 col-md-2">曲名</th>
                <th class="col-xs-2 col-sm-2 col-md-2">作詞</th>
                <th class="col-xs-2 col-sm-2 col-md-2">作曲</th>
                <th class="col-xs-2 col-sm-2 col-md-2">歌手</th>
                <th class="col-xs-2 col-sm-2 col-md-2">利用日</th>          
            </tr>
            <?php if (!empty($history)) { ?>
                <?php foreach ($history as $val) { ?>
                    <tr>
                        <td><a href="#"><?php echo $val->box_id ?></a></td>
                        <td><a href="#"><?php echo $val->song_id ?></a></td>
                        <td><?php echo $val->song_title ?></td>
                        <td><?php echo $val->lyricist ?></td>
                        <td><?php echo $val->composer ?></td>
                        <td><?php echo $val->singer ?></td>
                        <td><?php echo $val->use_datetime ?></td>
                    </tr>
                    <?php
                }
            }
            ?>
        </tbody>       
    </table>
</div>
<?php echo $this->load->view('common/footer') ?>