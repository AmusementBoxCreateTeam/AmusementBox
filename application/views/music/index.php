<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <h2>音楽一覧</h2>
    <?php
    $attributes = array('class' => 'form-inline', 'role' => 'form', 'method' => 'get');
    echo form_open('user', $attributes);
    ?>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">検索</div>
        </div>
        <div class="panel-body">
            <table class="table table-bordered">
                <tr>
                    <th>曲名</th><td> <input name="song_title" type="text" placeholder="曲名"  value="<?php echo!empty($search['song_title']) ? $search['song_title'] : ''; ?>"/></td>
                </tr>
                <tr>
                    <th>作詞</th><td> <input name="lyricist" type="text" placeholder="作詞"  value="<?php echo!empty($search['lyricist']) ? $search['lyricist'] : ''; ?>"/></td>
                </tr>
                <tr>
                    <th>作曲</th><td> <input name="composer" type="text" placeholder="作曲"  value="<?php echo!empty($search['composer']) ? $search['composer'] : ''; ?>"/></td>
                </tr>
                <tr>
                    <th>歌手</th><td> <input name="singer" type="text" placeholder="歌手"  value="<?php echo!empty($search['singer']) ? $search['singer'] : ''; ?>"/></td>
                </tr>
                <tr>
                    <th>ジャンル</th><td> <input name="genre" type="text" placeholder="ジャンル"  value="<?php echo!empty($search['genre']) ? $search['genre'] : ''; ?>"/></td>
                </tr>
                <tr>
                    <th>リリース日</th>
                    <td>
                        <div class="input-group">
                            <!--<span class="input-group-addon"><i class="glyphicon glyphicon-calendar"></i></span>-->
                            <input id="datepicker1" class="input-sm"  name="release_date_over" type="text" placeholder="登録日（以上）" readonly value="<?php echo!empty($search['entry_date_over']) ? $search['entry_date_over'] : ''; ?>"/>
                        </div>
                        <div class="input-group">
                            &nbsp;～&nbsp;
                            <!--<span class="input-group-addon"><i class="glyphicon glyphicon-calendar"></i></span>-->
                            <input id="datepicker2" class="input-sm" name="releases_date_under" type="text" placeholder="登録日（以下）" readonly value="<?php echo!empty($search['entry_date_under']) ? $search['entry_date_under'] : ''; ?>"/>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-info btn-lg"><i class="glyphicon glyphicon-search"></i>検索</button></div>
    </div>
    <input name="per_page" type="hidden" valeu="<?php echo!empty($per_page) ? $per_page : ''; ?>" />
</form>
<h4><?php echo $count_row . '件中&nbsp;' . $start_row . '～' . $end_row . '表示' ?></h4>
<table class="table table-hover">
    <tbody>
        <tr class="success">
            <th class="col-xs-3 col-sm-3 col-md-3">曲名</th>
            <th class="col-xs-2 col-sm-2 col-md-2">作詞</th>
            <th class="col-xs-2 col-sm-2 col-md-2">作曲</th>
            <th class="col-xs-2 col-sm-2 col-md-2">歌手</th>
            <th class="col-xs-1 col-sm-1 col-md-1">曲の長さ</th>
            <th class="col-xs-1 col-sm-1 col-md-1">ジャンル</th>
            <th class="col-xs-2 col-sm-2 col-md-2">リリース日</th>
            <th class="col-xs-1 col-sm-1 col-md-1">編集</th>
            <th class="col-xs-1 col-sm-1 col-md-1">削除</th>
        </tr>
        <?php if (!empty($list)) { ?>
            <?php foreach ($list as $val) { ?>
                <tr>
                    <td><?php echo $val->song_title ?></td>
                    <td><?php echo $val->lyricist ?></td>
                    <td><?php echo $val->composer ?></td>
                    <td><?php echo $val->singer ?></td>
                    <td><?php echo $val->song_time ?></td>
                    <td><?php echo $val->genre ?></td>
                    <td><?php echo $val->release_date ?></td>
                    <td><button class="btn-success" type="button" name="edit" value="編集" onclick="location.href='<?php echo base_url().'index.php/music/input/'.$val->id ?>'">編集</button></td>
                    <td><button class="btn-danger" type="button" name="delete" value="削除" onclick="delete_conf('<?php echo $val->id ?>','<?php echo $val->song_title ?>')">削除</button></td>
                </tr>
                <?php
            }
        }
        ?>
    </tbody>
</table>
<h4><?php echo $count_row . '件中&nbsp;' . $start_row . '～' . $end_row . '表示' ?></h4>
<?php echo!empty($paging) ? $paging : '' ?>
</div>
<script type="text/javascript">
    <!--
function delete_conf(id,title){
	if(window.confirm(title+'を削除してもよろしいですか？')){
		location.href = "<?php echo base_url().'index.php/music/delete_temp/'?>"+id;
        }
}
    -->
</script>
<?php $this->load->view('common/footer'); ?>
