<?php $this->load->view('common/header'); ?>
<?php echo form_open('statistic/rank5'); ?>
<div class="container-fluid">
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">音楽ランキングTOP5</div>
        </div>
        <div class="panel-body">
            <select name="gender">
                <option value="" <?php echo set_select('gender', ''); ?> >男女の選択</option>
                <option value="1" <?php echo set_select('gender', '1'); ?>>男性</option>
                <option value="0" <?php echo set_select('gender', '0'); ?>>女性</option>
            </select>
            <select name="age">
                <option valeu="" <?php echo set_select('age', ''); ?>>年代の選択</option>
                <option value="10" <?php echo set_select('age', '10'); ?>>10代</option>
                <option value="20" <?php echo set_select('age', '20'); ?>>20代</option>
                <option value="30" <?php echo set_select('age', '30'); ?>>30代</option>
                <option value="40" <?php echo set_select('age', '40'); ?>>40代</option>
                <option value="50" <?php echo set_select('age', '50'); ?>>50代</option>
                <option value="60" <?php echo set_select('age', '60'); ?>>60代</option>
                <option value="70" <?php echo set_select('age', '70'); ?>>70代</option>
                <option value="80" <?php echo set_select('age', '80'); ?>>80代</option>
                <option value="90" <?php echo set_select('age', '90'); ?>>90代</option>
            </select>
            </form>
            <div class="text-left" style="margin:10px;"><button type="submit" class="btn btn-info"><i class="glyphicon glyphicon-search"></i>検索</button></div>
            <table class="table table-bordered">
                <?php if (!empty($list)) { ?>
                    <tr class="danger">
                        <th class="col-xs-1 col-sm-1 col-md-1">順位</th>
                        <th class="col-xs-2 col-sm-2 col-md-2">曲名</th>
                        <th class="col-xs-2 col-sm-2 col-md-2">作詞</th>
                        <th class="col-xs-2 col-sm-2 col-md-2">作曲</th>
                        <th class="col-xs-2 col-sm-2 col-md-2">歌手</th>
                        <th class="col-xs-1 col-sm-1 col-md-1">ジャンル</th>
                        <th class="col-xs-1 col-sm-1 col-md-1">利用者数</th>
                        <th class="col-xs-1 col-sm-1 col-md-1">詳細</th>
                    </tr>
                    <?php
                    $i = 1;
                    foreach ($list as $val) {
                        ?>
                        <tr>
                            <td><?php echo $i ?></td>
                            <td><?php echo $val->song_title ?></td>
                            <td><?php echo $val->lyricist ?></td>
                            <td><?php echo $val->composer ?></td>
                            <td><?php echo $val->singer ?></td>
                            <td><?php echo $val->genre ?>   </td>
                            <td><?php echo $val->used_num ?>   </td>
                            <td><a href="<?php base_url().'index.php/music/detail' ?>">詳細</a></td>
                        </tr>
                        <?php
                        $i++;
                    }
                    ?>
                <?php } ?>
            </table>
            <a href="#">全ランキングを表示する</a>
        </div>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>
